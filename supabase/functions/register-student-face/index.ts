// Follow this setup guide to integrate the Deno language server with your editor:
// https://deno.land/manual/getting_started/setup_your_environment
// This enables autocomplete, go to definition, etc.

// Setup type definitions for built-in Supabase Runtime APIs
import "jsr:@supabase/functions-js/edge-runtime.d.ts";
import { getServiceRoleClient, getUserId, isStudent } from "../_shared/auth.ts";

console.log("Hello from Functions!");

Deno.serve(async (req) => {
  const client = await getServiceRoleClient();
  const userId = await getUserId(req);

  if (!userId) {
    return new Response(
      JSON.stringify({ error: "Unauthorized" }),
      { status: 401, headers: { "Content-Type": "application/json" } },
    );
  }

  const { embedding } = await req.json();
  if (
    typeof embedding !== "object" || !Array.isArray(embedding) ||
    embedding.length !== 512 || !embedding.every((n) =>
      typeof n === "number"
    )
  ) {
    return new Response(
      JSON.stringify({ error: "Embedding must be an array of 1536 numbers" }),
      {
        status: 400,
        headers: { "Content-Type": "application/json" },
      },
    );
  }

  if (!(await isStudent(req))) {
    return new Response(
      JSON.stringify({ error: "Unauthorized" }),
      { status: 401, headers: { "Content-Type": "application/json" } },
    );
  }

  const studentFace = { StudentId: userId, Embedding: embedding };

  const { data: insertData, error } = await client
    .from("StudentFaces")
    .upsert([studentFace])
    .select()
    .single();

  if (error) {
    return new Response(
      JSON.stringify({ error: "Insert failed", detail: error }),
      {
        status: 500,
        headers: { "Content-Type": "application/json" },
      },
    );
  }
  return new Response(
    JSON.stringify({ data: insertData }),
    { headers: { "Content-Type": "application/json" } },
  );
});
