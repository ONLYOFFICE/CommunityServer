namespace Ical.Net.Collections.Interfaces.Proxies
{
    public interface IGroupedCollectionProxy<TGroup, TOriginal, TNew> :
        IGroupedCollection<TGroup, TNew>
        where TOriginal : class, IGroupedObject<TGroup>
        where TNew : class, TOriginal
    {
        IGroupedCollection<TGroup, TOriginal> RealObject { get; }
        void SetProxiedObject(IGroupedCollection<TGroup, TOriginal> realObject);        
    }
}
