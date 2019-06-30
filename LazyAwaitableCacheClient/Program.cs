using LazyAwaitableCache;
using System;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LazyAwaitableCacheClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Cache<string> cache = new Cache<string>(TimeSpan.FromSeconds(3));

            try
            {

                string encached1 = cache.GetOrCreateItem(
                    "key",
                    () =>
                    Task.Delay(2000).ContinueWith<string>(s =>
                    {
                        
                        throw new Exception("Andi");
                    })).Result;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            try
            {
                string encached = cache.GetItem("key").Result;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            

            var dictionary = new ConcurrentDictionary<string, string>();
            // dictionary.GetOrAdd()
            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}
