namespace BioDomes.Domains.Entities;

public class UserAccount
{
    public int Id { get; set; }
    
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public DateOnly BirthDate { get; set; }
    
    public string? AvatarPath { get; set; }
    public string? ResearchOrganisation { get; set; } 
    public bool IsAdmin  { get; set; }
}