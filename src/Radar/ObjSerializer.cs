using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Radar
{
    public class ObjSerializer
    {
        public Model3D Deserialize(TextReader reader)
        {
            Model3D model = new Model3D();
            Regex regex = new Regex(@"
                ^(
                (?<v>v(?<v1>\ +(-?\d+(\.\d+)?))(?<v2>\ +(-?\d+(\.\d+)?))(?<v3>\ +(-?\d+(\.\d+)?)))|
                (?<vt>vt(?<vt1>\ +(-?\d+(\.\d+)?))(?<vt2>\ +(-?\d+(\.\d+)?))(?<vt3>\ +(-?\d+(\.\d+)?))?)|
                (?<vn>vn(?<vn1>\ +(-?\d+(\.\d+)?))(?<vn2>\ +(-?\d+(\.\d+)?))(?<vn3>\ +(-?\d+(\.\d+)?)))|
                (?<f>f(?<f1>\ +(?<f11>\d+)/(?<f12>\d*)(/(?<f13>\d+))?)(?<f2>\ +(?<f21>\d+)/(?<f22>\d*)(/(?<f23>\d+))?)(?<f3>\ +(?<f31>\d+)/(?<f32>\d*)(/(?<f33>\d+))?)(?<f4>\ +(?<f41>\d+)/(?<f42>\d*)(/(?<f43>\d+))?)?)|
                .*
                ).*$", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

            foreach (Match match in regex.Matches(reader.ReadToEnd()))
            {
                if (match.Groups["v"].Success)
                {
                    model.AddVector(
                        float.Parse(match.Groups["v1"].Value),
                        float.Parse(match.Groups["v2"].Value),
                        float.Parse(match.Groups["v3"].Value));
                }
                else if (match.Groups["vt"].Success)
                {
                    if (match.Groups["vt3"].Success)
                    {
                        model.AddTextureCoord(
                            float.Parse(match.Groups["vt1"].Value),
                            float.Parse(match.Groups["vt2"].Value),
                            float.Parse(match.Groups["vt3"].Value));
                    }
                    else
                    {
                        model.AddTextureCoord(
                            float.Parse(match.Groups["vt1"].Value),
                            float.Parse(match.Groups["vt2"].Value));
                    }
                }
                else if (match.Groups["vn"].Success)
                {
                    model.AddNormal(
                        float.Parse(match.Groups["vn1"].Value),
                        float.Parse(match.Groups["vn2"].Value),
                        float.Parse(match.Groups["vn3"].Value));
                }
                else if (match.Groups["f"].Success)
                {
                    if (match.Groups["f4"].Success)
                    {
                        model.AddFace(
                            int.Parse(match.Groups["f11"].Value),
                            match.Groups["f12"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f12"].Value),
                            match.Groups["f13"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f13"].Value),
                            int.Parse(match.Groups["f21"].Value),
                            match.Groups["f22"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f22"].Value),
                            match.Groups["f23"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f23"].Value),
                            int.Parse(match.Groups["f31"].Value),
                            match.Groups["f32"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f32"].Value),
                            match.Groups["f33"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f33"].Value),
                            int.Parse(match.Groups["f41"].Value),
                            match.Groups["f42"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f42"].Value),
                            match.Groups["f43"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f43"].Value));
                    }
                    else
                    {
                        model.AddFace(
                            int.Parse(match.Groups["f11"].Value),
                            match.Groups["f12"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f12"].Value),
                            match.Groups["f13"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f13"].Value),
                            int.Parse(match.Groups["f21"].Value),
                            match.Groups["f22"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f22"].Value),
                            match.Groups["f23"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f23"].Value),
                            int.Parse(match.Groups["f31"].Value),
                            match.Groups["f32"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f32"].Value),
                            match.Groups["f33"].Value.TrimEnd() == "" ? -1 : int.Parse(match.Groups["f33"].Value));
                    }
                }
            }

            model.NormalizeVectors();
            return model;
        }
    }
}
