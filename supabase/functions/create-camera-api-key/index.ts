import {getServiceRoleClient, isAdmin} from "../_shared/auth.ts";
import {createClient, SupabaseClient} from "supabase";
import "jsr:@supabase/functions-js/edge-runtime.d.ts"


async function getKey(supabaseClient: SupabaseClient){
    const key_resp = await supabaseClient.rpc('create_api_key');
    
    if (key_resp.error) {
        throw new Error('Failed to create API key: ' + key_resp.error);
    }
    
    const resp_hash = await supabaseClient.rpc('hash_apikey',
        { input_text: key_resp.data }
    );
    
    if (resp_hash.error) {
        throw new Error('Failed to hash API key: ' + resp_hash.error);
    }
    
    const token = key_resp.data;
    const hash = resp_hash.data;
    const prefix = token.slice(0, 8);
    const plaintextKey = `${prefix}_${token}`;
    return { prefix, plaintextKey, hash };
}

Deno.serve(async (req: Request) => {
    try {
        const authorized = await isAdmin(req)
        if (!authorized) {
            return new Response(
                JSON.stringify({ error: "Unauthorized" }),
                { status: 401, headers: { "Content-Type": "application/json" } },
            );
        }
        
        if (req.method !== "POST") {
            return new Response(
                JSON.stringify({ error: "Method must be POST" }),
                { status: 405, headers: { "Content-Type": "application/json" } },
            );
        }

        const { CameraId, Primary } = await req.json().catch(() => ({}));
        
        if (typeof Primary !== "boolean") {
            return new Response(
                JSON.stringify({ error: "Primary must be boolean" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }
        
        if (typeof CameraId !== "string") {
            return new Response(
                JSON.stringify({ error: "CameraId must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }

        const supabaseClient = await getServiceRoleClient();

        const { prefix, plaintextKey, hash } = await getKey(supabaseClient)
        
        const camera_resp = await supabaseClient
            .from('Cameras')
            .select('*')
            .eq('Id', CameraId)
            .single();
        
        if (camera_resp.error || !camera_resp.data) {
            return new Response(
                JSON.stringify({ error: "Camera not found", detail: camera_resp.error }),
                { status: 404, headers: { "Content-Type": "application/json" } },
            );
        }

        const apiKeyResp = await supabaseClient
            .from('ApiKeys')
            .insert({
                Name: camera_resp.data.Name + (Primary ? " Primary" : " Secondary"),
                Prefix: prefix,
                Hash: hash,
                IsActive: true,
            })
            .select()
            .single();

        if (!apiKeyResp.data) {
            return new Response(
                JSON.stringify({ error: "Insert failed", detail: apiKeyResp.error }),
                {
                    status: apiKeyResp.status || 500,
                    headers: { "Content-Type": "application/json" },
                },
            );
        }

        const cameraApiKeyResp = await supabaseClient
            .from('CameraApiKeys')
            .upsert({
                CameraId: CameraId,
                ApiKeyId: apiKeyResp.data.Id,
                Role: Primary ? 1 : 2,
            })
            .select()
        
        console.log(cameraApiKeyResp.data);
        
        if (cameraApiKeyResp.error || !cameraApiKeyResp.data) {
            return new Response(
                JSON.stringify({ error: "Insert failed", detail: cameraApiKeyResp.error }),
                {
                    status: cameraApiKeyResp.status || 500,
                    headers: { "Content-Type": "application/json" },
                },
            );
        }

        return new Response(
            JSON.stringify({ 
                Key: plaintextKey,
                CameraApiKey: cameraApiKeyResp.data
            }), 
            {
                status: 201,
                headers: { "Content-Type": "application/json" },
            });
    } catch (err) {
        return new Response(
            JSON.stringify({ error: "Internal error", detail: String(err) }),
            { status: 500, headers: { "Content-Type": "application/json" } },
        );
    }
});
