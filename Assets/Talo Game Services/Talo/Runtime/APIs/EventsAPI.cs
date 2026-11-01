using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaloGameServices
{
    public class EventsAPI : BaseAPI
    {
        public event Action OnFlushed;

        internal List<Event> queue = new ();
        private readonly int minQueueSize = 10;

        internal List<Event> eventsToFlush = new ();
        private bool lockFlushes;
        private bool flushAttemptedDuringLock;

        public EventsAPI() : base("v1/events") { }

        private string GetWindowMode()
        {
            if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
            {
                return "Exclusive fullscreen";
            }
            else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                return "Fullscreen window";
            }
            else if (Screen.fullScreenMode == FullScreenMode.MaximizedWindow)
            {
                return "Maximized window";
            }
            else if (Screen.fullScreenMode == FullScreenMode.Windowed)
            {
                return "Windowed";
            }

            return "";
        }

        private Prop[] BuildMetaProps()
        {
            return new Prop[]
                {
                    new Prop(("META_OS", SystemInfo.operatingSystem)),
                    new Prop(("META_GAME_VERSION", Application.version)),
                    new Prop(("META_WINDOW_MODE", GetWindowMode())),
                    new Prop(("META_SCREEN_WIDTH", Screen.width.ToString())),
                    new Prop(("META_SCREEN_HEIGHT", Screen.height.ToString()))
                };
        }

        public async Task Track(string name, params (string, string)[] props)
        {
            Talo.IdentityCheck(); //Identifica si existe un alias y un player, si no existe lanza una excepción.

            var ev = new Event
            {
                name = name
            }; //Define el evento con el nombre proporcionado

            ev.props = props
                .Select((propTuple) => new Prop(propTuple))
                .Concat(BuildMetaProps())
                .ToArray(); //Inicializa las propiedades de ese evento con las propiedades proporcionadas y las propiedades meta del sistema, las cuales son: .Select ((propTuple)), que lo que hace es recorrer cada propiedad proporcionada y crear un objeto Prop a partir de ella, y luego concatena esas propiedades con las propiedades meta del sistema, que se obtienen llamando al método BuildMetaProps().



            ev.timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(); //Inicializa la propiedad timestamp del evento con el tiempo actual en milisegundos desde la época Unix, para registrar cuándo ocurrió el evento.
            queue.Add(ev); //Agrega el evento a la cola de eventos pendientes de enviar, debido a que no se envía inmediatamente, sino que se acumulan varios eventos antes de enviarlos en un solo lote para optimizar el rendimiento y reducir la cantidad de solicitudes de red.

            if (queue.Count >= minQueueSize)
            {
                await Flush();
            } //Si la cantidad de eventos en la cola alcanza o supera el tamaño mínimo de la cola, se llama al método Flush() para enviar los eventos acumulados al servidor.
        }

        public async Task Flush()
        {
            Talo.IdentityCheck();

            if (lockFlushes)
            {
                flushAttemptedDuringLock = true;
                return;
            }

            var eventsToSend = queue.ToArray();
            if (eventsToSend.Length == 0)
            {
                return;
            }

            var uri = new Uri(baseUrl);
            var content = JsonUtility.ToJson(new EventsPostRequest(eventsToSend));

            try
            {
                lockFlushes = true;
                eventsToFlush.AddRange(eventsToSend);
                queue.Clear();

                await Call(uri, "POST", content);
                OnFlushed.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            finally
            {
                eventsToFlush.Clear();
                lockFlushes = false;
            }
            
            if (flushAttemptedDuringLock)
            {
                flushAttemptedDuringLock = false;
                await Flush();
            }
        }

        public void ClearQueue()
        {
            queue.Clear();
            eventsToFlush.Clear();
            lockFlushes = false;
            flushAttemptedDuringLock = false;
        }
    }
}
