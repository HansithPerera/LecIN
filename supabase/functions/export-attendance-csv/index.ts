import "jsr:@supabase/functions-js/edge-runtime.d.ts";
import { getServiceRoleClient, isAdmin } from "../_shared/auth.ts";

Deno.serve(async (req) => {
  const authorized = await isAdmin(req);
  if (!authorized) {
    return new Response(
      JSON.stringify({ error: "Unauthorized" }),
      { status: 401, headers: { "Content-Type": "application/json" } },
    );
  }

  const { Code, SemesterCode, Year } = await req.json().catch(() => ({}));

  if (typeof Code !== "string") {
    return new Response(
      JSON.stringify({ error: "Code must be string" }),
      { status: 400, headers: { "Content-Type": "application/json" } },
    );
  }

  if (typeof SemesterCode !== "number") {
    return new Response(
      JSON.stringify({ error: "Semester must be number" }),
      { status: 400, headers: { "Content-Type": "application/json" } },
    );
  }

  if (typeof Year !== "number") {
    return new Response(
      JSON.stringify({ error: "Year must be number" }),
      { status: 400, headers: { "Content-Type": "application/json" } },
    );
  }

  const supabaseClient = await getServiceRoleClient();
  const { data, error } = await supabaseClient
    .from("Courses")
    .select(
      "Code, SemesterCode, Year, Name, Classes( Id, StartTime, EndTime, Location, Attendance ( Timestamp, Students ( * ) ) )",
    )
    .eq("Code", Code)
    .eq("SemesterCode", SemesterCode)
    .eq("Year", Year);

  if (error) {
    console.error(error);
    return new Response(
      JSON.stringify({ error: "Query failed", detail: error }),
      {
        status: 500,
        headers: { "Content-Type": "application/json" },
      },
    );
  }

  if (!data || data.length === 0) {
    return new Response(
      JSON.stringify({ error: "Course not found" }),
      {
        status: 404,
        headers: { "Content-Type": "application/json" },
      },
    );
  }

  const course = data[0];
  const csv = exportAttendanceToCSV(course);

  return new Response(
    JSON.stringify({ csv }),
    { headers: { "Content-Type": "application/json" } },
  );
});

function exportAttendanceToCSV(course: any): string {
  const headers = [
    "CourseCode",
    "CourseName",
    "Semester",
    "Year",
    "ClassId",
    "ClassStart",
    "ClassEnd",
    "Location",
    "StudentId",
    "FirstName",
    "LastName",
    "AttendanceTimestamp",
  ];

  const rows: string[] = [];

  for (const cls of course.Classes) {
    if (cls.Attendance.length === 0) {
      rows.push(
        [
          course.Code,
          course.Name,
          course.SemesterCode,
          course.Year,
          cls.Id,
          cls.StartTime,
          cls.EndTime,
          cls.Location,
          "",
          "",
          "",
          "", // blanks for Student + Timestamp
        ].map((v) => JSON.stringify(v ?? "")).join(","),
      );
    } else {
      for (const att of cls.Attendance) {
        rows.push(
          [
            course.Code,
            course.Name,
            course.SemesterCode,
            course.Year,
            cls.Id,
            cls.StartTime,
            cls.EndTime,
            cls.Location,
            att.Students.Id,
            att.Students.FirstName,
            att.Students.LastName,
            att.Timestamp,
          ].map((v) => JSON.stringify(v ?? "")).join(","),
        );
      }
    }
  }

  return [headers.join(","), ...rows].join("\n");
}
