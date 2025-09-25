import { encodeHex } from "https://deno.land/std@0.224.0/encoding/hex.ts";
import {isAdmin} from "../_shared/auth.ts";
import { createClient } from 'npm:@supabase/supabase-js@2'

function toHex(bytes: Uint8Array): string {
    return encodeHex(bytes);
}

function randomHex(size: number): string {
    const bytes = new Uint8Array(size);
    crypto.getRandomValues(bytes);
    return toHex(bytes);
}

async function sha256Hex(input: string): Promise<string> {
    const data = new TextEncoder().encode(input);
    const digest = await crypto.subtle.digest("SHA-256", data);
    return toHex(new Uint8Array(digest));
}

async function getKey(){
    const prefix = randomHex(8); // 16 hex chars
    const token = randomHex(32); // 64 hex chars
    const plaintextKey = `${prefix}_${token}`;
    const hash = await sha256Hex(plaintextKey);
    return { prefix, plaintextKey, hash };
}

Deno.serve(async (req: Request) => {
    try {
        const authHeader = req.headers.get('Authorization')!
        const token = authHeader.replace('Bearer ', '')
        const authorized = await isAdmin(token)
        if (authorized) {
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

        const body = await req.json().catch(() => ({}));
        
        const name =
            typeof body.name === "string" && body.name.trim().length
                ? body.name.trim()
                : null;

        const { prefix, plaintextKey, hash } = await getKey()

        const supabaseClient = createClient(
            Deno.env.get('SUPABASE_URL') ?? '',
            Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
        );
        
        let resp = await supabaseClient
            .from('ApiKeys')
            .insert({
                Name: name,
                Prefix: prefix,
                Hash: hash,
                IsActive: true,
            })
            .select()
            .single();
        
        console.log(resp)

        if (!resp.data) {
            return new Response(
                JSON.stringify({ error: "Insert failed", detail: resp.error }),
                {
                    status: resp.status || 500,
                    headers: { "Content-Type": "application/json" },
                },
            );
        }
        return new Response(
            JSON.stringify({ 
                key: plaintextKey 
            }), 
            {
                status: 201,
                headers: { "Content-Type": "application/json" },
            });
    } catch (err) {
        console.error("Request processing failed:", err);
        return new Response(
            JSON.stringify({ error: "Internal error", detail: String(err) }),
            { status: 500, headers: { "Content-Type": "application/json" } },
        );
    }
});
