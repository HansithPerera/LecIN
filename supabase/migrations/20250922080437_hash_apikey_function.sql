set check_function_bodies = off;

CREATE OR REPLACE FUNCTION public.hash_apikey(input_text character varying)
 RETURNS character varying
 LANGUAGE plpgsql
AS $function$BEGIN
    RETURN encode(digest(input_text, 'sha256'), 'base64');
END;$function$
;


