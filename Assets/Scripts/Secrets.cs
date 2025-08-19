using UnityEngine;

[CreateAssetMenu(fileName = "Secrets", menuName = "Custom/Secrets")]
public class Secrets : ScriptableObject
{
    public string metapersonClientId;
    public string metapersonClientSecret;
    public string supabaseUrl;
    public string supabaseAnonKey;
}