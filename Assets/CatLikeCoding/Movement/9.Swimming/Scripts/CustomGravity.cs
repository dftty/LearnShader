using UnityEngine;
using System.Collections.Generic;

namespace Swimming
{
    public static class CustomGravity
    {

        public static List<GravitySource> sources = new List<GravitySource>();

        public static void Register(GravitySource source)
		{
			Debug.Assert(
				!sources.Contains(source),
				"Duplicate registration of gravity source!", source
			);

			sources.Add(source);
		}

		public static void UnRegister(GravitySource source)
		{
			Debug.Assert(
				sources.Contains(source), 
				"Unregistration of unknown gravity source!", source);
			sources.Remove(source);
		}

        public static Vector3 GetGravity(Vector3 position)
        {
            Vector3 gravity = Vector3.zero;

            foreach (GravitySource source in sources)
            {
                gravity += source.GetGravity(position);
            }

            return gravity;    
        }

        public static Vector3 GetUpAxis(Vector3 position)
        {
            Vector3 gravity = Vector3.zero;

            foreach (GravitySource source in sources)
            {
                gravity += source.GetGravity(position);
            }

            return -gravity.normalized;  
        }

        public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
        {
            Vector3 gravity = Vector3.zero;

            foreach (GravitySource source in sources)
            {
                gravity += source.GetGravity(position);
            }

            upAxis = -gravity.normalized;
            return gravity; 
        }
    }
}