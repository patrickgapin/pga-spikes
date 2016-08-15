using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail.Helpers
{
    public class Constants
    {
        public const string MyActorSystem = "MyActorSystem";
        public class FilePaths
        {
            public static string ValidationActorPath => $"akka://{MyActorSystem}/user/validationActor";
            public static string TailCoordinatorActorPath => $"akka://{MyActorSystem}/user/tailCoordinatorActor";
        }
    }
}
