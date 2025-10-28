insert into storage.buckets
(id, name, public)
values
    ('studentfaces', 'studentfaces', false);

CREATE TABLE IF NOT EXISTS "public"."StudentFaces" (
                                                       "StudentId" UUID PRIMARY KEY,
                                                       "FaceImagePath" VARCHAR NOT NULL,
                                                       "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NULL,
    CONSTRAINT "StudentFaces_StudentId_fkey" FOREIGN KEY ("StudentId") REFERENCES "public"."Students"("Id") ON DELETE CASCADE
    );

ALTER TABLE "public"."StudentFaces" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "public"."StudentFaces" OWNER TO "postgres";