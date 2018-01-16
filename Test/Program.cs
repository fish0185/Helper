using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Helper;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeSpan time = StopwatchUtil.Time(() =>
            {
                for (var i = 0; i < 10000000; i++)
                {
                    var p = ObjectCreator<Person>.InstanceNew(); // 1062.9373 ms
                }
            });
            Console.WriteLine("ObjectCreator<Person>.InstanceNew() " + time.TotalMilliseconds);

            TimeSpan time1 = StopwatchUtil.Time(() =>
            {
                for (var i = 0; i < 10000000; i++)
                {
                    var p = new Person(); //  69.5146 ms
                }
            });
            Console.WriteLine("new " + time1.TotalMilliseconds);

            TimeSpan time2 = StopwatchUtil.Time(() =>
            {
                for (var i = 0; i < 10000000; i++)
                {
                    var p = ObjectCreator<Person>.InstanceNewFunc(); // 1185.4388 ms
                }
            });
            Console.WriteLine("ObjectCreator<Person>.InstanceNewFunc() " + time2.TotalMilliseconds);

            TimeSpan time3 = StopwatchUtil.Time(() =>
            {
                for (var i = 0; i < 10000000; i++)
                {
                    var p = ObjectCreator<Person>.InstanceActivatorFunc(); // 1011.0356 ms
                }
            });
            Console.WriteLine("ObjectCreator<Person>.InstanceActivatorFunc() " + time3.TotalMilliseconds);

            TimeSpan time4 = StopwatchUtil.Time(() =>
            {
                for (var i = 0; i < 10000000; i++)
                {
                    var p = ObjectCreator<Person>.InstanceFormatterServices();
                }
            });
            Console.WriteLine("ObjectCreator<Person>.InstanceFormatterServices() " + time4.TotalMilliseconds);

            TimeSpan time5 = StopwatchUtil.Time(() =>
            {
                for (var i = 0; i < 10000000; i++)
                {
                    var p = ObjectCreator<Person>.Instance(); // 201.2314
                }
            });
            Console.WriteLine("Expression  " + time5.TotalMilliseconds);

            TimeSpan timeActivator = StopwatchUtil.Time(() =>
            {
                for (var i = 0; i < 10000000; i++)
                {
                    var p = ObjectCreator<Person>.InstanceActivator(); // 1086.5959
                }

            });
            Console.WriteLine("ObjectCreator<Person>.InstanceActivator()" + timeActivator.TotalMilliseconds);

            // https://vagifabilov.wordpress.com/2010/04/02/dont-use-activator-createinstance-or-constructorinfo-invoke-use-compiled-lambda-expressions/
            // https://stackoverflow.com/questions/6582259/fast-creation-of-objects-instead-of-activator-createinstancetype
            // create object using expression
            Func<Person> factory = Expression.Lambda<Func<Person>>(Expression.New(typeof(Person))).Compile();
            var p2 =  factory();



            var personType = typeof(Person);
            var types = new[]
            {
                typeof(int),
                typeof(string)
            };

            var ctor = personType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis,
                types, null);


            ParameterInfo[] paramsInfo =  ctor.GetParameters();

            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp = new Expression[paramsInfo.Length];

            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }


            NewExpression newExp = Expression.New(ctor, argsExp);

            LambdaExpression lambda = Expression.Lambda(typeof(Person), newExp, param);
        }
    }

    class Person
    {
        public int Age { get; set; }

        public string Name { get; set; }

        public Person() : this(0, "Gary")
        {

        }

        public Person(int age, string name)
        {
            Age = age;
            Name = name;
        }
    }
}
