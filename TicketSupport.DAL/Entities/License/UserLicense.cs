using System;
namespace TicketSupport.DAL.Entities.License
{
/// <summary>
/// Represents a software license.
/// </summary>
public class UserLicense:CommonFields
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// Gets or sets the license key.
    /// </summary>
    public string Key { get; set; }

    
    /// <summary>
    /// Gets or sets the activation date of the license.
    /// </summary>
    public DateTime ActivationDate { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the license.
    /// </summary>
    public DateTime ExpirationDate { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the license is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets the remaining days of the license.
    /// </summary>
    public int RemainingDays
    {
        get
        {
            if (ExpirationDate < DateTime.Now)
            {
                return 0;
            }
            else
            {
                return (ExpirationDate - DateTime.Now).Days;
            }
        }
    }

    /// <summary>
    /// Validates the license.
    /// </summary>
    /// <returns>true if the license is valid; otherwise, false.</returns>
    public bool Validate()
    {
        return !string.IsNullOrEmpty(Key) && ExpirationDate >= DateTime.Now && IsActive;
    }
}
}