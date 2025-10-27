// Follow this setup guide to integrate the Deno language server with your editor:
// https://deno.land/manual/getting_started/setup_your_environment
// This enables autocomplete, go to definition, etc.

// Setup type definitions for built-in Supabase Runtime APIs
import { getServiceRoleClient } from "../_shared/auth.ts";

import "jsr:@supabase/functions-js/edge-runtime.d.ts"

Deno.serve(async (req) => {
    const client = await getServiceRoleClient();
    const apiKey = req.headers.get("apikey");
    
    if (!apiKey) {
        return new Response(
            JSON.stringify({ error: "API key is required" }),
            { status: 401, headers: { "Content-Type": "application/json" } },
        );
    }
    
    if (req.method !== "POST") {
        return new Response(
            JSON.stringify({ error: "Method must be POST" }),
            { status: 405, headers: { "Content-Type": "application/json" } },
        );
    }

    const { error, data: hash} = await client.rpc('hash_apikey',
        { input_text: apiKey }
    );
    
    if (error) {
        return new Response(
            JSON.stringify({ error: "Failed to hash API key", detail: error }),
            { status: 500, headers: { "Content-Type": "application/json" } },
        );
    }
    
    const { error: cameraApiKeyError, data: cameraApiKey } = await client
        .from("CameraApiKeys")
        .select("*, Cameras(*), ApiKeys!inner(*)")
        .eq("ApiKeys.Hash", hash)
        .limit(1);
    
    if (cameraApiKeyError || !cameraApiKey) {
        return new Response(
            JSON.stringify({ error: "Invalid API key", detail: cameraApiKeyError }),
            { status: 401, headers: { "Content-Type": "application/json" } },
        );
    }
    
    const camera = cameraApiKey[0]?.Cameras;
    
    if (!camera || !camera.IsActive) {
        return new Response(
            JSON.stringify({ error: "Camera is inactive or not found" }),
            { status: 403, headers: { "Content-Type": "application/json" } },
        );
    }

    const { Embedding } = await req.json().catch(() => ({}));
    
    if (!Embedding || !Array.isArray(Embedding) || Embedding.length === 0) {
        return new Response(
            JSON.stringify({ error: "Embedding is required and must be a non-empty array" }),
            { status: 400, headers: { "Content-Type": "application/json" } },
        );
    }
    
    const resp = await client.rpc("FindClosestStudentFace", {
        target_embedding: Embedding,
        max_distance: 0.7,
    });
    
    if (resp.error) {
        return new Response(
            JSON.stringify({ error: "Failed to find closest student face", detail: resp.error }),
            { status: 500, headers: { "Content-Type": "application/json" } },
        );
    }
    
    const closestFace = resp.data[0];
    
    if (!closestFace) {
        return new Response(
            JSON.stringify({ error: "No matching student face found" }),
            { status: 404, headers: { "Content-Type": "application/json" } },
        );
    }
    
    return new Response(
        JSON.stringify({ 
            studentId: closestFace.StudentId,
            message: "Camera check-in successful", 
            cameraId: camera.Id }),
        { headers: { "Content-Type": "application/json" } },
    );
})

