BEGIN;
SELECT plan(2);

select has_table( 'Admins' );
select col_is_pk( 'Admins', 'Id' );

SELECT * FROM finish();
ROLLBACK;
