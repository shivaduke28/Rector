using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

namespace Rector.Osc
{
    /// <summary>
    /// このマシンの IPv4 アドレス。
    ///
    /// 受信は IPAddress.Any なので待ち受けには関係しないが、OSC には探索の仕組みが無く、
    /// 送信側には宛先を人間が打ち込むことになる。iPad を持ったまま Mac のターミナルへ
    /// 戻らずに済むよう、起動ログに「ここへ送れ」を出すために使う。
    /// </summary>
    public static class OscLocalAddress
    {
        public static string[] GetIPv4Addresses()
        {
            try
            {
                var candidates = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up
                                 && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(ni => ni.GetIPProperties())
                    .ToArray();

                // Docker や仮想マシンのブリッジはゲートウェイを持たない。混ざると
                // どれを iPad に打てばいいのか分からなくなるので、他の端末から
                // 届く見込みのある口だけに絞る。全部落ちたら絞らずに出す
                var routable = candidates.Where(p => p.GatewayAddresses.Count > 0).ToArray();
                var chosen = routable.Length > 0 ? routable : candidates;

                return chosen
                    .SelectMany(p => p.UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .Distinct()
                    .ToArray();
            }
            catch (Exception e)
            {
                // 宛先の案内が出せないだけで受信はできる。起動を止める理由にはしない
                Debug.LogWarning($"Failed to enumerate local IPv4 addresses: {e.Message}");
                return Array.Empty<string>();
            }
        }
    }
}
