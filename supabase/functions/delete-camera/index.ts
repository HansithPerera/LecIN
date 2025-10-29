import { getServiceRoleClient, isAdmin } from "../_shared/auth.ts";
import "jsr:@supabase/functions-js/edge-runtime.d.ts";

Deno.serve(async (req) => {
    const authorized = await isAdmin(req);
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

    const { CameraId } = await req.json();

    const supabaseClient = await getServiceRoleClient();

    const { data, error } = await supabaseClient
        .from("Cameras")
        .delete()
        .eq("Id", CameraId)
        .select()

    if (error || !data) {
        return new Response(
            JSON.stringify({ error: "Delete failed", detail: error }),
            {
                status: 500,
                headers: { "Content-Type": "application/json" },
            },
        );
    }   
    
    //deleted response, no content
    return new Response(
        null,
        { status: 204 }
    );
});
