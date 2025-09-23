import { createClient } from 'supabase'
import {isAdmin} from "../_shared/auth.ts";
import "jsr:@supabase/functions-js/edge-runtime.d.ts"

Deno.serve(async (req) => {
    const authHeader = req.headers.get('Authorization')!
    const token = authHeader.replace('Bearer ', '')
    const authorized = await isAdmin(token)
    if (!authorized) {
        return new Response(
            JSON.stringify({error: "Unauthorized"}),
            {status: 401, headers: {"Content-Type": "application/json"}},
        );
    }
    
    if (req.method !== "POST") {
        return new Response(
            JSON.stringify({error: "Method must be POST"}),
            {status: 405, headers: {"Content-Type": "application/json"}},
        );
    }
    
    const { Name, Location } = await req.json()
    
    if (typeof Name !== "string" || !Name.trim().length) {
        return new Response(
            JSON.stringify({ error: "Name is required" }),
            { status: 400, headers: { "Content-Type": "application/json" } },
        );
    }
    
    if (typeof Location !== "string" || !Location.trim().length) {
        return new Response(
            JSON.stringify({ error: "Location is required" }),
            { status: 400, headers: { "Content-Type": "application/json" } },
        );
    }
    
    const supabaseClient = createClient(
        Deno.env.get('SUPABASE_URL') ?? '',
        Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    );

    const { data, error } = await supabaseClient
        .from('Cameras')
        .insert([
            { Name: Name, Location: Location, IsActive: true },
        ])
        .select()
    
    if (error || !data) {
        return new Response(
            JSON.stringify({ error: "Insert failed", detail: error }),
            {
                status: 500,
                headers: { "Content-Type": "application/json" },
            },
        );
    }
    
    return new Response(
        JSON.stringify(data),
        { headers: { "Content-Type": "application/json" } },
    );
})
