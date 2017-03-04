﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UnstuckMEInterfaces
{
    public interface IServer
    {
		[OperationContract]
		void GetMessage();

        [OperationContract]
        void GetUpdate(int value, UnstuckME_Classes.UserInfo user);
    }
}
