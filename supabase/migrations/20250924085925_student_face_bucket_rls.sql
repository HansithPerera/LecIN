create policy "Allow Students to view own face 1ytg7rv_0"
on "storage"."objects"
as permissive
for select
to public
using (((bucket_id = 'studentfaces'::text) AND (EXISTS ( SELECT 1
   FROM "StudentFaces"
  WHERE (("StudentFaces"."StudentId" = auth.uid()) AND (objects.name = ("StudentFaces"."FaceImagePath")::text))))));



