#if UNITY_EDITOR
namespace Kamgam.MouseShortcuts
{
    public interface IHistory
    {
        void Init(
            System.Action<IHistory,double> onNewEntryAdded,
            System.Action<IHistory, double> onEntryRestored
            );

        void DeInit();

        /// <summary>
        /// Returns -1 if no entry with a time > referenceTime was found.
        /// </summary>
        /// <param name="referenceTime"></param>
        /// <returns>A positive number (>= 0) or -1 if none was found</returns>
        double GetNextTimeDelta(double referenceTime);

        /// <summary>
        /// Returns -1 if no entry with a time < referenceTime was found.
        /// </summary>
        /// <param name="referenceTime"></param>
        /// <returns>A positive number (>= 0) or -1 if none was found</returns>
        double GetPreviousTimeDelta(double referenceTime);

        /// <summary>
        /// Selects the next entry relative to referenceTime.
        /// </summary>
        /// <param name="referenceTime"></param>
        /// <returns>The time of the selected entry or -1 if none was found.</returns>
        double SelectNext(double referenceTime);

        /// <summary>
        /// Selects the previous entry relative to referenceTime.
        /// </summary>
        /// <returns>The time of the selected entry or -1 if none was found.</returns>
        double SelectPrevious(double referenceTime);
    }
}
#endif