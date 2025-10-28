import {getServiceRoleClient, isTeacher} from "../_shared/auth.ts";
import "jsr:@supabase/functions-js/edge-runtime.d.ts"

Deno.serve(async (req: Request) => {
    try {
        const authorized = await isTeacher(req)
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

        const { ClassId, StudentId, Reason } = await req.json().catch(() => ({}));
        
        const supabaseClient = await getServiceRoleClient();
        
        if (typeof ClassId !== "string") {
            return new Response(
                JSON.stringify({ error: "ClassId must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }
        
        if (typeof StudentId !== "string") {
            return new Response(
                JSON.stringify({ error: "StudentId must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }
        
        if (typeof Reason !== "string") {
            return new Response(
                JSON.stringify({ error: "Reason must be string" }),
                { status: 400, headers: { "Content-Type": "application/json" } },
            );
        }
        
        const existingAttendanceResp = await supabaseClient
            .from('Attendance')
            .select('*')
            .eq('ClassId', ClassId)
            .eq('StudentId', StudentId)
            .single();
        
        if (existingAttendanceResp.error && existingAttendanceResp.status !== 406) {
            console.error(existingAttendanceResp.error);
            return new Response(
                JSON.stringify({ error: "Query failed", detail: existingAttendanceResp.error }),
                {
                    status: 500,
                    headers: { "Content-Type": "application/json" },
                },
            );
        }
        
        if (existingAttendanceResp.data) {
            const updateResp = await supabaseClient
                .from('Attendance')
                .update({
                    Reason: Reason,
                })
                .eq('ClassId', existingAttendanceResp.data.ClassId)
                .eq('StudentId', existingAttendanceResp.data.StudentId)
                .select()
                .single();
            if (updateResp.error) {
                console.error(updateResp.error);
                return new Response(
                    JSON.stringify({ error: "Update failed", detail: updateResp.error }),
                    {
                        status: 500,
                        headers: { "Content-Type": "application/json" },
                    },
                );
            }
            return new Response(
                JSON.stringify({ attendance: updateResp.data }),
                { headers: { "Content-Type": "application/json" } },
            );
            
        } else {
            const insertResp = await supabaseClient
                .from('Attendance')
                .insert({
                    ClassId: ClassId,
                    StudentId: StudentId,
                    Timestamp: (new Date()).toISOString(),
                    Reason: Reason,
                })
                .select()
                .single();
            if (insertResp.error) {
                console.error(insertResp.error);
                return new Response(
                    JSON.stringify({ error: "Insert failed", detail: insertResp.error }),
                    {
                        status: 500,
                        headers: { "Content-Type": "application/json" },
                    },
                );
            }
            return new Response(
                JSON.stringify({ attendance: insertResp.data }),
                { headers: { "Content-Type": "application/json" } },
            );
        }
    } catch (err) {
        return new Response(
            JSON.stringify({ error: "Internal error", detail: String(err) }),
            { status: 500, headers: { "Content-Type": "application/json" } },
        );
    }
});
