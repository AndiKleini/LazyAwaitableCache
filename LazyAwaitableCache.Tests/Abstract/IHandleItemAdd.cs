using System.Threading.Tasks;

namespace LazyAwaitableCache.Tests.Abstract
{
    /// <summary>
    /// Implements an event handler interface for 
    /// the ItemAdd event.
    /// </summary>
    public interface IHandleItemAdd
    {
        /// <summary>
        /// Specifies an operation to handle the ItemAdd event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        Task OnItemAdd(object sender, AddItemEventArgs args);
    }
}
