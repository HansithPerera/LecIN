set check_function_bodies = off;

CREATE OR REPLACE FUNCTION public.create_api_key()
 RETURNS character varying
 LANGUAGE plpgsql
AS $function$BEGIN
RETURN encode(gen_random_bytes(32), 'hex') AS api_key;
END;$function$
;


