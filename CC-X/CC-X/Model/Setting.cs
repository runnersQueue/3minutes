﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace CC_X.Model
{
    class Setting : World, Serializer
    {
        public Setting(SettingType setting, Vector3 position)
        {
            type = WorldType.Setting;
            IsDead = false;
        }        
        public override void UpdatePos(Vector3 position)
        {
            throw new NotImplementedException();
        }

        // Store information concerning the environment
        public string Serialize()
        {
            //string info = string.Format("{0}, {1}", setting, id);
            //return info;
            throw new NotImplementedException();
        }

        // Load the environment
        public void DeSerialize(string fileinfo)
        {
            throw new NotImplementedException();
        }
    }
}
