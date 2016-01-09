using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amatsukaze.HelperClasses
{

    public class MessagetoGUI
    {
        public string Message { get; set; }
    }

    public interface ISubscriber<TEventType>
    {
        void OnEventHandler(TEventType e);
    }

    public interface IEventAggregator
    {
        void PublishEvent<TEventType>(TEventType eventToPublish);

        void SubscribeEvent(Object subscriber);
    }

    public class EventAggregator : IEventAggregator
    {
        private Dictionary<Type, List<WeakReference>> eventSubscribers = new Dictionary<Type, List<WeakReference>>();

        private readonly object lockSubscriberDictionary = new Object();

        public void PublishEvent<TEventType>(TEventType eventToPublish)
        {
            var subscriberType = typeof(ISubscriber<>).MakeGenericType(typeof(TEventType));

            var subscribers = GetSubscriberList(subscriberType);

            List<WeakReference> subscribersToBeRemoved = new List<WeakReference>();

            foreach (var weakSubscriber in subscribers)
            {
                if (weakSubscriber.IsAlive)
                {
                    var subscriber = (ISubscriber <TEventType>) weakSubscriber.Target;

                    InvokeSubscriberEvent<TEventType>(eventToPublish, subscriber);
                }
                else
                {
                    subscribersToBeRemoved.Add(weakSubscriber);
                }
            }

            if (subscribersToBeRemoved.Any())
            {
                lock (lockSubscriberDictionary)
                {
                    foreach (var remove in subscribersToBeRemoved)
                    {
                        subscribers.Remove(remove);
                    }
                }
            }                 
        }        

        public void SubscribeEvent(object subscriber)
        {
            lock (lockSubscriberDictionary)
            {
                var subscriberTypes = subscriber.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubscriber<>));

                WeakReference weakReference = new WeakReference(subscriber);

                foreach (var subscriberType in subscriberTypes)
                {
                    List <WeakReference> subscribers = GetSubscriberList(subscriberType);

                    subscribers.Add(weakReference);
                }
            }
        }

        private void InvokeSubscriberEvent<TEventType>(TEventType eventtoPublish, ISubscriber<TEventType> subscriber)
        {
            SynchronizationContext syncContext = SynchronizationContext.Current;

            if (syncContext == null)
            {
                syncContext = new SynchronizationContext();
            }

            syncContext.Post(s => subscriber.OnEventHandler(eventtoPublish), null);
        }

        private List<WeakReference> GetSubscriberList(Type subscriberType)
        {
            List<WeakReference> subscribersList = null;

            lock (lockSubscriberDictionary)
            {
                bool found = this.eventSubscribers.TryGetValue(subscriberType, out subscribersList);

                if (!found)
                {
                    subscribersList = new List<WeakReference>();

                    this.eventSubscribers.Add(subscriberType, subscribersList);
                }
            }

            return subscribersList;
        }
    }
}
