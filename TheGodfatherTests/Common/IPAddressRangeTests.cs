﻿using System.IO;
using NUnit.Framework;
using TheGodfather.Common;
using WebSocket4Net.Command;

namespace TheGodfather.Tests.Common
{
    [TestFixture]
    public sealed class IPAddressRangeTests
    {
        [Test]
        public void TryParseSuccessTests()
        {
            AssertParseSuccess("123.123.132.123");
            AssertParseSuccess("123.123.132.123:10480", 10480);
            AssertParseSuccess("255.123.132.123:10480", 10480);
            AssertParseSuccess("123.255.132.123:10480", 10480);
            AssertParseSuccess("123.123.255.123:10480", 10480);
            AssertParseSuccess("123.123.123.255:10480", 10480);
            AssertParseSuccess("123.123.123.0:10480", 10480);
            AssertParseSuccess("123.123.0.123:10480", 10480);
            AssertParseSuccess("123.0.123.123:10480", 10480);
            AssertParseSuccess("43.2.123.123:10480", 10480);
            AssertParseSuccess("12.12.12.12:10480", 10480);
            AssertParseSuccess("12.12.12.12:1000", 1000);
            AssertParseSuccess("12.12.12.12:10000", 10000);
            AssertParseSuccess("12.12.12.12:20000", 20000);
            AssertParseSuccess("12.12.12.12:65535", 65535);
            AssertParseSuccess("1.1.1.1:10480", 10480);
            AssertParseSuccess("123.123.132.123:1000", 1000);
            AssertParseSuccess("123.123.132.123:65535", 65535);
            AssertParseSuccess("123.123.132.123");
            AssertParseSuccess("255.123.132.123");
            AssertParseSuccess("123.255.132.123");
            AssertParseSuccess("123.123.255.123");
            AssertParseSuccess("123.123.123.255");
            AssertParseSuccess("123.012.132.123");
            AssertParseSuccess("123.123.012.123");
            AssertParseSuccess("123.123.123.012");
            AssertParseSuccess("123.00.132.123");
            AssertParseSuccess("123.123.00.123");
            AssertParseSuccess("123.123.123.00");
            AssertParseSuccess("123.000.132.123");
            AssertParseSuccess("123.123.000.123");
            AssertParseSuccess("123.123.123.000");
            AssertParseSuccess("123.123.123.0");
            AssertParseSuccess("123.123.0.123");
            AssertParseSuccess("123.0.123.123");
            AssertParseSuccess("123.123.132");
            AssertParseSuccess("123.123.123.2");
            AssertParseSuccess("123.123.2.123");
            AssertParseSuccess("123.2.123.123");
            AssertParseSuccess("2.123.123.123");
            AssertParseSuccess("123.123.132");
            AssertParseSuccess("1.1.1.1");
            AssertParseSuccess("123.123");
            AssertParseSuccess("255.123");
            AssertParseSuccess("123.255");
            AssertParseSuccess("255.23");
            AssertParseSuccess("23.255");
            AssertParseSuccess("43.22");
            AssertParseSuccess("123.2");
            AssertParseSuccess("2.123");
            AssertParseSuccess("123.0");

            foreach (string ip in File.ReadAllLines(Path.Join("Common", "IPTestData", "ips.txt"))) {
                AssertParseSuccess(ip);
                AssertParseSuccess(ip + ":12345", 12345);
            }


            static void AssertParseSuccess(string text, ushort port = 0)
            {
                Assert.That(IPAddressRange.TryParse(text, out IPAddressRange? parsed), Is.True);
                Assert.That(parsed, Is.Not.Null);
                Assert.That(parsed!.CompleteRange, Is.EqualTo(text));

                int sepIndex = text.IndexOf(':');
                string range = sepIndex == -1 ? text : text.Substring(0, sepIndex);
                Assert.That(parsed!.Range, Is.EqualTo(range));
                Assert.That(parsed!.Port, Is.EqualTo(port));
            }
        }

        [Test]
        public void TryParseFailTests()
        {
            AssertParseFail("asd123.123.132.123:10480");
            AssertParseFail("123.123.132.123:10480asd");
            AssertParseFail("asd123.123.132.123:10480asd");
            AssertParseFail("0.0.0.0:10480");
            AssertParseFail("0.123.123.123:10480");
            AssertParseFail("00.123.123.123:10480");
            AssertParseFail("256.156.156.156:10480");
            AssertParseFail("156.256.156.156:10480");
            AssertParseFail("156.156.256.156:10480");
            AssertParseFail("156.156.156.256:10480");
            AssertParseFail("156.156.156.156:");
            AssertParseFail("156.156.156.156:0");
            AssertParseFail("156.156.156.156:9");
            AssertParseFail("156.156.156.156:65536");
            AssertParseFail("156.156.156.156:99999");
            AssertParseFail("156.156.156.156:100000");
            AssertParseFail("156.156.156.156:0000");
            AssertParseFail("156.156.156.156:0001");
            AssertParseFail("156.156.156.156:0090");
            AssertParseFail("156.156.156.156:00000");
            AssertParseFail("156.156.156.156:00001");
            AssertParseFail("256.156.156.156");
            AssertParseFail("156.256.156.156");
            AssertParseFail("156.156.256.156");
            AssertParseFail("156.156.156.256");
            AssertParseFail("123.123.132.123:");
            AssertParseFail("255.123.132.");
            AssertParseFail("0.255.132.123");
            AssertParseFail("123..255.123");
            AssertParseFail("123.123..255");
            AssertParseFail(".123.123.0");
            AssertParseFail("256.123.123");
            AssertParseFail("...2");
            AssertParseFail("123.123.");
            AssertParseFail("1.1.");
            AssertParseFail("123.");
            AssertParseFail("256.123");
            AssertParseFail("123.256");
            AssertParseFail("0.123");
            AssertParseFail("256");
            AssertParseFail("255");
            AssertParseFail("123");
            AssertParseFail("1");


            static void AssertParseFail(string text)
            {
                Assert.That(IPAddressRange.TryParse(text, out IPAddressRange? parsed), Is.False);
                Assert.That(parsed, Is.Null);
            }
        }
    }
}
