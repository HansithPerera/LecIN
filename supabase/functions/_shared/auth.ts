import {createClient} from 'npm:@supabase/supabase-js@2'

export async function isAdmin(token: string): Promise<boolean> {
    const supabaseClient = createClient(
        Deno.env.get('SUPABASE_URL') ?? '',
        Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    );

    const user = await supabaseClient.auth.getUser();
    const admin = await supabaseClient
        .from('Admins')
        .select('*')
        .eq('id', user.data.user?.id)
        .single();
    return !!user.data.user && admin.data !== null;
}