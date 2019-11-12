using System;
using System.Collections.Generic;
using System.Text;


namespace psai.net
{
    public class UnityVersionComparer
    {
        public enum UnityVersionType
        {
            unknown = 0,    // numbers matter for comparison
            beta = 1,
            final = 2,
            patch = 3
        }

        public enum ComparisonResult
        {
            unknown,
            earlier,
            equal,
            later
        }

        public class UnityVersion
        {
            /// <summary>
            /// e.g. in Unity 5.4.2  this is 5
            /// </summary>
            public int MajorVersionNumber { get; private set; }

            /// <summary>
            /// E.g. in Unity 5.4.2 this is 4
            /// </summary>
            public int MiddleVersionNumber { get; private set; }

            /// <summary>
            /// E.g. in Unity 5.4.2 this is 2
            /// </summary>
            public int MinorVersionNumber { get; private set; }

            public UnityVersionType UnityVersionType { get; private set; }

            /// <summary>
            /// E.g. in Unity 5.4.2f1 this is 0. in Unity 5.4.2p6 this is 6
            /// </summary>
            public int PatchOrBetaVersion { get; private set; }


            public UnityVersion(int major, int middle, int minor, int patchOrBetaVersion = 0, UnityVersionType versionType = UnityVersionType.final)
            {
                MajorVersionNumber = major;
                MiddleVersionNumber = middle;
                MinorVersionNumber = minor;
                PatchOrBetaVersion = patchOrBetaVersion;
                UnityVersionType = versionType;
            }

            /// <summary>
            /// Pass Application.unityVersion to create a string off the current Application
            /// </summary>
            /// <param name="unityVersionString"></param>
            public UnityVersion(string unityVersionString = "")
            {
                if (unityVersionString == "")
                {
                    unityVersionString = UnityEngine.Application.unityVersion;
                }

                UnityVersionType = UnityVersionType.unknown;
                int majorVersion = -1;
                int middleVersion = -1;
                int minorVersion = -1;
                int patchOrBetaVersion = -1;

                string[] tokens = unityVersionString.Split('.');
                if (tokens.Length == 3)
                {
                    int.TryParse(tokens[0], out majorVersion);
                    int.TryParse(tokens[1], out middleVersion);

                    char[] delimiters = { 'b', 'f', 'p' };

                    string[] endSubstrings = tokens[2].Split(delimiters);

                    if (endSubstrings.Length > 0)
                    {
                        string minorString = endSubstrings[0];
                        if (int.TryParse(minorString, out minorVersion))
                        {
                            UnityVersionType = UnityVersionType.final;
                            patchOrBetaVersion = 0;
                        }

                        if (endSubstrings.Length > 1)
                        {
                            string patchString = endSubstrings[1];
                            if (int.TryParse(patchString, out patchOrBetaVersion))
                            {
                                if (tokens[2].Contains("f"))
                                {
                                    UnityVersionType = UnityVersionType.final;
                                }
                                else if (tokens[2].Contains("b"))
                                {
                                    UnityVersionType = UnityVersionType.beta;
                                }
                                else if (tokens[2].Contains("p"))
                                {
                                    UnityVersionType = UnityVersionType.patch;
                                }
                            }
                        }
                    }
                    else
                    {
                        patchOrBetaVersion = 0;
                        UnityVersionType = UnityVersionType.final;
                    }

                    MajorVersionNumber = majorVersion;
                    MiddleVersionNumber = middleVersion;
                    MinorVersionNumber = minorVersion;
                    PatchOrBetaVersion = patchOrBetaVersion;
                }
            }
        }

        public static ComparisonResult CompareCurrentVersionAgainst(string versionString)
        {
            UnityVersion local = new UnityVersion(UnityEngine.Application.unityVersion);
            UnityVersion other = new UnityVersion(versionString);
            return CompareUnityVersions(local, other);
        }


        public static ComparisonResult CompareUnityVersions(string firstVersionString, string secondVersionString)
        {
            UnityVersion first = new UnityVersion(firstVersionString);
            UnityVersion second = new UnityVersion(secondVersionString);
            return CompareUnityVersions(first, second);
        }

        /// <summary>
        /// Returns the ComparionResult depending of the first in relation to the second.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="ignorePatchVersion"></param>
        /// <returns></returns>
        public static ComparisonResult CompareUnityVersions(UnityVersion first,
                                                            UnityVersion second,
                                                            bool ignorePatchVersion = false)
        {

            if (first.MajorVersionNumber <= 0 || second.MajorVersionNumber <= 0)
            {
                return ComparisonResult.unknown;
            }

            ComparisonResult majorResult = CompareSubVersion(first.MajorVersionNumber, second.MajorVersionNumber);

            if (majorResult != ComparisonResult.equal)
            {
                return majorResult;
            }

            ComparisonResult middleResult = CompareSubVersion(first.MiddleVersionNumber, second.MiddleVersionNumber);
            if (middleResult != ComparisonResult.equal)
            {
                return middleResult;
            }

            ComparisonResult minorResult = CompareSubVersion(first.MinorVersionNumber, second.MinorVersionNumber);
            if (minorResult != ComparisonResult.equal)
            {
                return minorResult;
            }

            if (ignorePatchVersion)
            {
                return ComparisonResult.equal;
            }
            else
            {
                return CompareBetaOrPatch(first, second);
            }
        }

        private static ComparisonResult CompareBetaOrPatch(UnityVersion first, UnityVersion second)
        {
            if (first.UnityVersionType == UnityVersionType.unknown || second.UnityVersionType == UnityVersionType.unknown)
            {
                return ComparisonResult.unknown;
            }
            else
            {
                int comparisonResult = ((int)first.UnityVersionType).CompareTo((int)second.UnityVersionType);
                if (comparisonResult == 0)
                {
                    return CompareSubVersion(first.PatchOrBetaVersion, second.PatchOrBetaVersion);
                }
                else if (comparisonResult > 0)
                {
                    return ComparisonResult.later;
                }

                return ComparisonResult.earlier;
            }
        }


        private static ComparisonResult CompareSubVersion(int first, int second)
        {
            if (first == second)
            {
                return ComparisonResult.equal;
            }

            if (first < second)
            {
                return ComparisonResult.earlier;
            }

            return ComparisonResult.later;
        }
    }
}
