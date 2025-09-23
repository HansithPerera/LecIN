import {createClient} from 'supabase';

export async function isAdmin(token: string): Promise<boolean> {
    const supabaseClient = createClient(
        Deno.env.get('SUPABASE_URL') ?? '',
        Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    );
    
    if (token === Deno.env.get('SUPABASE_SERVICE_ROLE_KEY')) {
        return true;
    }

    const user = await supabaseClient.auth.getUser(token);
    const admin = await supabaseClient
        .from('Admins')
        .select('*')
        .eq('Id', user.data.user?.id)
        .single();
    return !!user.data.user && admin.data !== null;
}
