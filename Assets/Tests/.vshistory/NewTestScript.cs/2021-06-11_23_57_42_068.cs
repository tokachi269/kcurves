using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Assets
{
    public class NewTestScript
    {
        [Test]
        public void PathCalculatingTest()
        {
            var go = new GameObject("Hoge");
            Path.Path path = go.gameObject.AddComponent<Path.Path>();

            path.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), 60, false);
            path.AddKnot(new Vector3(1, 2, 1), new Quaternion(0, 0.5f, 0, 1), 60, false);
            path.AddKnot(new Vector3(0, 2, 5), new Quaternion(0, 0, 0, 1), 60, true);
            path.AddKnot(new Vector3(0, -2, 1), new Quaternion(0, 0, 1, 1), 60, false);
            path.AddKnot(new Vector3(0, 1, 4), new Quaternion(0, 0, 0, 1), 60, false);
            path.AddLookAt(new Vector3(0, 4, 0), new Quaternion(0, 0, 0, 1), 60);

            path.Output(step:10, isLoop:false);

            Assert.AreEqual(path.BeziersCount, 6);
            Assert.AreEqual(path.BeziersPoints, 13);
                
            Assert.AreApproximatelyEqual(path.BeziersTotalLength, 2.66914f);

        }

        [Test]
        public void RemoveKnotsTest()
        {
            var go = new GameObject("Hoge");
            Path.Path path = go.gameObject.AddComponent<Path.Path>();

            path.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), 60, false);
            path.AddKnot(new Vector3(1, 2, 1), new Quaternion(0, 0.5f, 0, 1), 60, false);
            path.AddKnot(new Vector3(0, 2, 5), new Quaternion(0, 0, 0, 1), 60, true);
            path.AddKnot(new Vector3(0, -2, 1), new Quaternion(0, 0, 1, 1), 60, false);
            path.AddKnot(new Vector3(0, 1, 4), new Quaternion(0, 0, 0, 1), 60, false);
            path.AddLookAt(new Vector3(0, 4, 0), new Quaternion(0, 0, 0, 1), 60);

            path.Output(step: 10, isLoop: false);


            path.RemoveKnot();
            Assert.AreEqual(path.BeziersCount, 4);
            Assert.AreEqual(path.BeziersPoints, 9);

           // path.RemoveKnot();
           // Assert.AreEqual(path.Beziers.SegmentCount, 2);
           // Assert.AreEqual(path.Beziers.Points.Length, 5);

        }
        [UnityTest]
        public IEnumerator CameraMoveTest()
        {
            var cd = new GameObject("Hoge");
            CameraDirector cameraDirector = cd.gameObject.AddComponent<CameraDirector>();

            Assert.IsTrue(0 <= cameraDirector.path.diffT);
            Assert.IsTrue(0 <= cameraDirector.path.dist);
            yield return null;
        }
    }
}