using System.Collections.Generic;
using Meta;
using UnityEngine;

namespace Meta
{

    /// <summary>  This class is used to support use of multiple rayCasts over an area. </summary> 
    public static class MultiRaycast
    {

        private static int _raycastBaseLayers = 0;

        /// <summary>
        /// IMPORTANT: If the names of the layers used change this must be updated to include the new layer names.
        /// </summary>
        public static int BaseLayers
        {
            get
            {
                if (_raycastBaseLayers == 0)
                {
                    _raycastBaseLayers = LayerMask.NameToLayer("Ignore Raycast");
                }
                return _raycastBaseLayers;
            }
        }

        /// <summary>
        /// Sends out multiple RayCasts over an area and returns all 
        /// the hits. Raycasts are divided into rows and each row
        /// is circle shaped.
        /// 
        /// </summary>
        /// <returns>An array of RaycastHits</returns>
        /// <param name="origin">Origin of the raycasts.</param>
        /// <param name="direction">The central direction.</param>
        /// <param name="rows">The number of rows of Raycast.</param>
        /// <param name="raysPerRow">RayCasts per row.</param>
        /// <param name="theta">The angle between direction and the outermost row.</param>
        /// <param name="layerMask">Layer mask.</param>
        /// <param name="descend">Whether or not to return child colliders.</param>
        public static RaycastHit[] MultiRayCast(Vector3 origin,
            Vector3 direction,
            int rows,
            int raysPerRow,
            float theta,
            LayerMask layerMask,
            bool descend = false)
        {
            int layers = layerMask.value;
            if (layers < 0)
            {
                layers = 0;
            }

            layers |= BaseLayers;
            layers = ~layers;
            if (rows == 0
                || raysPerRow == 0)
            {
                return new RaycastHit[0];
            }
            RaycastHit[] result = new RaycastHit[rows*raysPerRow];

            Vector3 crossVector = Vector3.up;
            Vector3 perpVector = Vector3.Cross(direction, crossVector);
            if (perpVector.magnitude == 0)
            {
                crossVector = Vector3.right;
                perpVector = Vector3.Cross(direction, crossVector);
            }

            Quaternion rotationWithinRow = Quaternion.AngleAxis(360f/raysPerRow, direction);

            for (int i = 0; i < rows; i++)
            {
                Quaternion rotationToCurrentRow = Quaternion.AngleAxis(((1.0f + i)/rows)*theta, perpVector);
                Vector3 currentRay = rotationToCurrentRow*direction;
                for (int j = 0; j < raysPerRow; j++)
                {
                    if (descend)
                    {
                        RaycastHit[] hits = UnityEngine.Physics.RaycastAll(origin, currentRay, Mathf.Infinity, layers);
                        if (hits.Length != 0)
                            result[i*raysPerRow + j] = CollidedDescendant(hits);
                    }
                    else
                    {
                        //Debug.DrawRay(origin,currentRay,Color.red);
                        UnityEngine.Physics.Raycast(origin, currentRay, out result[i*raysPerRow + j], Mathf.Infinity, layers);
                    }
                    currentRay = rotationWithinRow*currentRay;
                }
            }

            return result;
        }

        /// <summary>
        /// An example function to deal with the output from MultiRayCast. 
        /// Simply returns the transform that was hit by the most raycasts.
        /// Can be used as the foundation for something cleverer.
        /// </summary>
        /// <returns>The transform with the most hits.</returns>
        /// <param name="hits">Output from MultiRayCast.</param>
        public static GameObject MostHit(RaycastHit[] hits)
        {

            Dictionary<Transform, int> results = new Dictionary<Transform, int>();
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null)
                {
                    if (!results.ContainsKey(hit.transform))
                    {
                        results[hit.transform] = 0;
                    }
                    results[hit.transform] += 1;
                }
            }
            Transform mostHit = null;
            int mostHitCount = 0;
            foreach (KeyValuePair<Transform, int> result in results)
            {
                if (result.Value > mostHitCount)
                {
                    mostHit = result.Key;
                    mostHitCount = result.Value;
                }
            }
            return mostHit != null ? mostHit.gameObject : null;
        }

        /// <summary>
        /// The object that was hit the most factoring in weights for each row.
        /// </summary>
        /// <returns>The gameobject that was hit the most.</returns>
        /// <param name="hits">Hits.</param>
        /// <param name="rowWeights">Row weights.</param>
        public static GameObject MostHitWithWeights(RaycastHit[] hits, float[] rowWeights)
        {
            if (hits.Length%rowWeights.Length != 0)
            {
                Debug.LogError("Invalid number of row weights.");
                return null;
            }

            Dictionary<Transform, float> results = new Dictionary<Transform, float>();
            int i = 0;
            int row = 0;
            int raysPerRow = hits.Length/rowWeights.Length;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null)
                {
                    if (!results.ContainsKey(hit.transform))
                    {
                        results[hit.transform] = 0;
                    }
                    results[hit.transform] += rowWeights[row];
                }
                i++;
                if (i != raysPerRow)
                    continue;
                row++;
                i = 0;
            }
            Transform mostHit = null;
            float mostHitCount = 0;
            foreach (KeyValuePair<Transform, float> result in results)
            {
                if (!(result.Value > mostHitCount))
                    continue;
                mostHit = result.Key;
                mostHitCount = result.Value;
            }
            return mostHit != null ? mostHit.gameObject : null;
        }

        /// <summary>
        /// Allows the results of a RaycastAll to "penetrate" through the colliders of the parents,
        /// which allows you to raycast into colliders inside of other colliders in a model. 
        /// </summary>
        /// <returns>The most decended collider that was hit.</returns>
        /// <param name="hits">The results of a RaycastAll.</param>
        private static RaycastHit CollidedDescendant(RaycastHit[] hits)
        {
            RaycastHit current = hits[0];
            for (int i = 1; i < hits.Length; i++)
            {
                if (hits[i].distance < current.distance && !current.transform.IsChildOf(hits[i].transform)
                    || hits[i].transform.IsChildOf(current.transform) && current.collider.bounds.Contains(hits[i].point))
                {
                    current = hits[i];
                }
            }
            return current;
        }
    }
}