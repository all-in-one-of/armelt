﻿using Meta.Internal.Playback;
using UnityEngine;


namespace Meta
{
    /// <summary>   The interaction engine factory. </summary>
    internal static class InteractionEngineFactory
    {
        /// <summary>   Creates the sources. </summary>
        /// <typeparam name="TPoint">   Type of the point. </typeparam>
        /// <param name="pointCloudSource">         The point cloud source. </param>
        /// <param name="handConsumerType">         Type of the hand consumer. </param>
        /// <param name="interactionDataSource">    The interaction data source. </param>
        /// <param name="playbackFolder">           Pathname of the playback folder. </param>
        internal static void CreateSources<TPoint>( out IPointCloudSource<TPoint> pointCloudSource,
                                                    out IPlaybackSource<PointCloudData<TPoint>> pcdPlaybackSource,
                                                    string handConsumerType,
                                                    string interactionDataSource,
                                                    string playbackFolder = "") where TPoint : PointXYZ, new()
        {
            switch (interactionDataSource)
            {
                case "Playback":
                    CreatePlaybackSources(out pointCloudSource, out pcdPlaybackSource, playbackFolder);
                    break;
                default:
                    CreateRealdataSources(out pointCloudSource);
                    pcdPlaybackSource = null;
                    break;
            }
        }

        /// <summary>   Creates playback sources. </summary>
        /// <typeparam name="TPoint">   Type of the point. </typeparam>
        /// <param name="pointCloudSource"> The point cloud source. </param>
        /// <param name="pcdPlaybackSource">   The playback source. </param>
        /// <param name="playbackFolder">   Pathname of the playback folder. </param>
        internal static void CreatePlaybackSources<TPoint>( out IPointCloudSource<TPoint> pointCloudSource,
                                                            out IPlaybackSource<PointCloudData<TPoint>> pcdPlaybackSource,
                                                            string playbackFolder) where TPoint : PointXYZ, new()
        {
            if (System.IO.Directory.Exists(playbackFolder))
            {
                ThreadedPlaybackPointCloudSource src;
                try
                {
                    src = new ThreadedPlaybackPointCloudSource(playbackFolder);
                }
                catch (System.IO.FileNotFoundException)
                {
                    src = new ThreadedPlaybackPointCloudSource();
                }
                pointCloudSource = src as IPointCloudSource<TPoint>;
                pcdPlaybackSource = src as IPlaybackSource<PointCloudData<TPoint>>;
            }
            else
            {
                UnityEngine.Debug.Log("Playback folder did not exist. Creating blank sources.");
                ThreadedPlaybackPointCloudSource pcdSrc = new ThreadedPlaybackPointCloudSource();
                pointCloudSource = pcdSrc as IPointCloudSource<TPoint>;
                pcdPlaybackSource = pcdSrc as IPlaybackSource<PointCloudData<TPoint>>;
            }
        }

        /// <summary>   Creates realdata sources. </summary>
        /// <typeparam name="TPoint">   Type of the point. </typeparam>
        /// <param name="pointCloudSource"> The point cloud source. </param>
        public static void CreateRealdataSources<TPoint>(out IPointCloudSource<TPoint> pointCloudSource) where TPoint : PointXYZ, new()
        {
            pointCloudSource = new PointCloudInterop<PointXYZConfidence>() as IPointCloudSource<TPoint>;
        }

        /// <summary>   Constructs the interaction Engine. </summary>
        /// <param name="interactionEngine">        The interaction engine. </param>
        /// <param name="handConsumerType">         Type of the hand consumer. </param>
        /// <param name="interactionDataSource">    The interaction data source. </param>
        /// <param name="playbackFolder">           Pathname of the playback folder. </param>
        public static void Construct(out InteractionEngine interactionEngine, string handConsumerType, string interactionDataSource, Transform depthOcclusionTransform, string playbackFolder = "")
        {
            IPointCloudSource<PointXYZConfidence> pointCloudSource; //todo: maybe support other stuff?
            IPlaybackSource<PointCloudData<PointXYZConfidence>> pcdPlaybackSource;
            CreateSources(out pointCloudSource, out pcdPlaybackSource, handConsumerType, interactionDataSource, playbackFolder);
            interactionEngine = new InteractionEngine(pointCloudSource, handConsumerType, depthOcclusionTransform, pcdPlaybackSource);
        }
    }
}
