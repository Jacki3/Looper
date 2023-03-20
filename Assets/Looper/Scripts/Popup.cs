using UnityEngine;
using MoreMountains.Feedbacks;

/// <summary>
/// A component to handle popups, their opening and closing
/// </summary>
public class Popup : MonoBehaviour
{
    public MMFeedbacks openFeedback;
    public MMFeedbacks closeFeedback;

    /// true if the popup is currently open
    public bool CurrentlyOpen = false;

    protected Animator _animator;


    /// <summary>
    /// On Start, we initialize our popup
    /// </summary>
    protected virtual void Start()
    {
        Initialization();
    }

    /// <summary>
    /// On Init, we grab our animator and store it for future use
    /// </summary>
    protected virtual void Initialization()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// On update, we update our animator parameter
    /// </summary>
    protected virtual void Update()
    {
        if (_animator != null)
        {
            _animator.SetBool("Closed", !CurrentlyOpen);
        }
    }

    /// <summary>
    /// Opens the popup
    /// </summary>
    public virtual void Open()
    {
        if (CurrentlyOpen)
        {
            return;
        }

        _animator.SetTrigger("Open");
        openFeedback?.PlayFeedbacks();
        CurrentlyOpen = true;
    }

    /// <summary>
    /// Closes the popup
    /// </summary>
    public virtual void Close()
    {
        if (!CurrentlyOpen)
        {
            return;
        }

        _animator.SetTrigger("Close");
        closeFeedback?.PlayFeedbacks();
        CurrentlyOpen = false;
    }

}