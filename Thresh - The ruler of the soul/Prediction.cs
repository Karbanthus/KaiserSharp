using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ThreshTherulerofthesoul
{
    class Prediction
    {
        static Obj_AI_Hero Player = ObjectManager.Player;
        static Vector3 predictPos;
        
        public static Tuple<Vector3, float, float> GetPredictedPos2(Obj_AI_Hero target, float _range, float _delay, float _width, float _speed = float.MaxValue)
        {
            float test = 100;
            float test2 = 100;
            //beta ver
            predictPos = new Vector3(0, 0, 0);
            float Time = 0;
            if (_speed != float.MaxValue && _speed > 0)
            {
                Time = Time + _delay;
            }
            else
            {
                Time = _delay;
            }

            var distance = Player.Distance(target.Position);
            var DashTime = distance / _speed + _delay;

            List<Vector2> waypoints = target.GetWaypoints();

            if (target.IsDashing())
            {
                var DashingDistance = target.GetDashInfo().Speed * DashTime;

                predictPos = target.Position + (target.GetDashInfo().EndPos.To3D() - target.Position).Normalized() * DashingDistance;
                if (target.Distance(predictPos) > target.Distance(target.GetDashInfo().EndPos.To3D()))
                {
                    Console.WriteLine("Dashing");
                    predictPos = target.GetDashInfo().EndPos.To3D();
                }
            }
            else if (waypoints.Count > 1)
            {
                for (int i = 0; i < waypoints.Count - 1; i++)
                {
                    var a = waypoints[i];
                    var b = waypoints[i + 1];
                    var UnitVector = (b - a).Normalized();

                    for (int j = 0; j < 100; j++)
                    {
                        Time = Time + 0.01f;
                        var dd = (a + UnitVector * target.MoveSpeed * Time).To3D();
                        var ff = Time * _speed;

                        if (Math.Abs(Player.Distance(dd) - ff) < 20)
                        {
                            /*
                            Drawing.OnDraw += delegate(EventArgs args)
                            {
                                var wtsX = Drawing.WorldToScreen(dd).X;
                                var wtsY = Drawing.WorldToScreen(dd).Y;
                                Render.Circle.DrawCircle(dd, 150, System.Drawing.Color.Yellow);
                                //Drawing.DrawText(wtsX, wtsY, System.Drawing.Color.Black, "" + j);
                            };*/
                            Console.WriteLine("Normal");
                            test = Player.Distance(dd);
                            test2 = ff;
                            predictPos = dd;
                            break;
                        }
                    }
                    if (!predictPos.IsZero)
                    {
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Non Move");
                predictPos = target.Position;
                test = 1;
                test2 = 1;
            }

            if (!predictPos.IsZero)
            {
                if (_range > Player.Distance(predictPos))
                {
                    return new Tuple<Vector3,float,float>(predictPos, test, test2);
                }
                else
                {
                    return new Tuple<Vector3,float,float>(predictPos = new Vector3(0, 0, 0), 1,1);
                }
            }
            else
            {
                return new Tuple<Vector3,float,float>(predictPos = new Vector3(0, 0, 0), 1,1);
            }
        }

    }
}
