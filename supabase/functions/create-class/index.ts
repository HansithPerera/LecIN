import { getServiceRoleClient, isTeacher } from "../_shared/auth.ts";
import "jsr:@supabase/functions-js/edge-runtime.d.ts";

Deno.serve(async (req: Request) => {
    try {
        const authorized = await isTeacher(req);
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

        const { CourseCode, CourseYear, CourseSemesterCode, StartTime, EndTime, Location } = await req.json().catch(() => ({}));

        const supabaseClient = await getServiceRoleClient();

        if (typeof CourseCode !== "string") {
            return new Response(
                JSON.stringify({ error: "CourseCode must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }

        if (typeof CourseYear !== "number") {
            return new Response(
                JSON.stringify({ error: "CourseYear must be number" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }

        if (typeof CourseSemesterCode !== "number") {
            return new Response(
                JSON.stringify({ error: "CourseSemesterCode must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }
        
        if (typeof StartTime !== "string") {
            return new Response(
                JSON.stringify({ error: "StartTime must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }

        if (typeof EndTime !== "string") {
            return new Response(
                JSON.stringify({ error: "EndTime must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }

        if (typeof Location !== "string") {
            return new Response(
                JSON.stringify({ error: "Location must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }

        const insertResp = await supabaseClient
            .from("Classes")
            .insert({
                CourseCode,
                CourseYear,
                CourseSemesterCode,
                StartTime,
                EndTime,
                Location,
            })
            .select()
            .single();

        if (insertResp.error) {
            console.error(insertResp.error);
            return new Response(
                JSON.stringify({ error: "Insert failed", detail: insertResp.error }),
                { status: 500, headers: { "Content-Type": "application/json" } },
            );
        }

        return new Response(
            JSON.stringify({ data: insertResp.data }),
            { status: 200, headers: { "Content-Type": "application/json" } },
        );
        
    } catch (err) {
        return new Response(
            JSON.stringify({ error: "Internal error", detail: String(err) }),
            { status: 500, headers: { "Content-Type": "application/json" } },
        );
    }
});
