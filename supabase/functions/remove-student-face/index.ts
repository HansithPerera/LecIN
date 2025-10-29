import "supabase";
import { getServiceRoleClient, getUserId, isStudent } from "../_shared/auth.ts";

Deno.serve(async (req) => {
  if (req.method !== "POST") {
    return new Response("Method Not Allowed", { status: 405 });
  }

  if (await isStudent(req) === false) {
    return new Response("Unauthorized", { status: 401 });
  }

  const userId = await getUserId(req);

  const client = await getServiceRoleClient();

  const { data, error } = await client
    .from("StudentFaces")
    .delete()
    .eq("StudentId", userId)
    .single();

  if (error) {
    return new Response(
      JSON.stringify({ error: error.message }),
      { headers: { "Content-Type": "application/json" }, status: 400 },
    );
  }
  
  return new Response(
    JSON.stringify({ message: "Student face data removed successfully" }),
    { headers: { "Content-Type": "application/json" }, status: 200 },
  );
});
