drop policy "Allow Students to view own face 1ytg7rv_0" on "storage"."objects";

create extension if not exists vector with schema extensions;
     
DELETE FROM storage.objects
WHERE bucket_id = 'studentfaces';

DELETE FROM storage.buckets
WHERE id = 'studentfaces';

alter table "public"."StudentFaces" drop column "FaceImagePath";

alter table "public"."StudentFaces" add column "Embedding" vector(512);

CREATE INDEX "StudentFaces_Embedding_idx" ON public."StudentFaces" USING ivfflat ("Embedding") WITH (lists='100');

set check_function_bodies = off;

CREATE OR REPLACE FUNCTION public."FindClosestStudentFace"(target_embedding vector, max_distance double precision)
 RETURNS SETOF "StudentFaces"
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT *
    FROM public."StudentFaces" s
    WHERE s."Embedding" <=> target_embedding < max_distance
    ORDER BY s."Embedding" <=> target_embedding
    LIMIT 1;
END;
$function$
;


