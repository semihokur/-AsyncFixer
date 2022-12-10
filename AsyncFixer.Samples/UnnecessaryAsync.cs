﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AsyncFixer.Samples
{
    internal class UnnecessaryAsync
    {
        public static async void foo1()
        {
            await Task.Delay(2000).ConfigureAwait(false);
        }

        public static async Task foo2()
        {
            var t = Task.Delay(2000);
            await t.ConfigureAwait(false);
        }

        public async Task<Stream> GetRequestStreamAsync()
        {
            return await GetRequestStreamAsync();
        }

        public async Task<int> GetRequestStreamAsync(int b)
        {
            //int c = await Task.Run(() => { return 3; });
            if (b > 5)
            {
                return 3;
            }
            return await GetRequestStreamAsync(b);
        }

        public async Task<int> boo(int b)
        {
            if (b > 5)
            {
                return await Task.Run(() => { return 3; });
            }
            return await boo(b);
        }

        public async Task<int> RequestAsync(int b)
        {
            using (new StreamReader(""))
            {
                return await RequestAsync(b).ConfigureAwait(false);
            }
        }

        public async Task<int> RequestAsyncTry(int b)
        {
            try
            {
                return await RequestAsync(b).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return await RequestAsync(b).ConfigureAwait(false);
            }
        }

        protected async void OnInitialize()
        {
            try
            {
                foo1();
                await Task.Delay(100);
                foo1();
            }
            catch (Exception)
            {
            }
            finally
            {
            }
        }

        public async Task<int> CreateTablesAsync(params Type[] types)
        {
            return await Task.Factory.StartNew(() => { return 3; });
        }

        public async Task<int> CreateTablesAsync2(params Type[] types)
        {
            try
            {
                return await Task.Factory.StartNew(() => { return 3; });
            }
            catch (Exception e)
            {
            }
            return 5;
        }

        public static async Task foo3(int i)
        {
            if (i > 2)
            {
                await Task.Delay(2000);
            }
        }

        public static async Task foo4(int i)
        {
            if (i > 2)
            {
                await Task.Delay(3000);
                await Task.Delay(2000);
            }
        }

        public async Task MyFunction()
        {
            await Task.Run(async () =>
            {
                await foreach (var i in RangeAsync(10, 3))
                {
                    Console.WriteLine(i);
                }
            });
        }

        static async IAsyncEnumerable<int> RangeAsync(int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                await Task.Delay(i);
                yield return start + i;
            }
        }

        Task foo()
        {
            using var destination = new MemoryStream();
            using FileStream source = File.Open("data", FileMode.Open);
            return source.CopyToAsync(destination);
       }
    }


    class Program
    {
        static async void foo()
        {
            var newStream = new FileStream("", FileMode.Create);

            using var stream3 = new FileStream("", FileMode.Create);
            stream3.ReadAsync(null).GetAwaiter().GetResult();
            var res = stream3.ReadAsync(null).Result;
            newStream.CopyToAsync(stream3).Wait();
            await newStream.CopyToAsync(stream3);
            newStream.CopyToAsync(stream3);
        }
    }
}