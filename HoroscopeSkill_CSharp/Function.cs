using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace HoroscopeSkill_CSharp
{
    public class Function
    {
        private class FortuneScore
        {
            public string Score { get; }
            public string Description { get; }

            public FortuneScore(string score, string description)
            {
                this.Score = score;
                this.Description = description;
            }
        }

        private readonly List<FortuneScore> _fortunes = new List<FortuneScore>
        {
            new FortuneScore("good","星みっつで良いでしょう"),
            new FortuneScore("normal","星ふたつで普通でしょう"),
            new FortuneScore("bad","星ひとつでイマイチでしょう"),
        };

        private readonly string[] _luckyColors =
        {
            "赤",
            "ピンク",
            "オレンジ",
            "ブルー",
            "水色",
            "紺色",
            "紫",
            "黒",
            "グリーン",
            "レモンイエロー",
            "ホワイト",
            "チャコールグレー"
        };

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest skillRequest, ILambdaContext context)
        {
            SkillResponse skillResponse = null;

            try
            {
                //型スイッチの利用
                switch (skillRequest.Request)
                {
                    case LaunchRequest launchRequest:
                        skillResponse = HelpIntentHandler(skillRequest);
                        break;
                    case IntentRequest intentRequest:
                        switch (intentRequest.Intent.Name)
                        {
                            case "HoroscopeIntent":
                                skillResponse = HoroscopeIntentHandler(skillRequest);
                                break;
                            case "LuckyColorIntent":
                                skillResponse = LuckyColorIntentHandler(skillRequest);
                                break;
                            case "AMAZON.HelpIntent":
                                skillResponse = HelpIntentHandler(skillRequest);
                                break;
                            case "AMAZON.CancelIntent":
                                skillResponse = CancelAndStopIntentHandler(skillRequest);
                                break;
                            case "AMAZON.StopIntent":
                                skillResponse = CancelAndStopIntentHandler(skillRequest);
                                break;
                            default:
                                //skillResponse = ErrorHandler(skillRequest);
                                break;
                        }

                        break;
                    case SessionEndedRequest sessionEndedRequest:
                        skillResponse = SessionEndedRequestHandler(skillRequest);
                        break;
                    default:
                        //skillResponse = ErrorHandler(skillRequest);
                        break;
                }
            }
            catch (Exception ex)
            {
                skillResponse = ErrorHandler(skillRequest);
            }

            return skillResponse;
        }




        #region 各インテント、リクエストに対応する処理を担当するメソッドたち

        private SkillResponse HoroscopeIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            //StarSignスロットから値を取り出します。
            var sign = intentRequest.Intent.Slots["StarSign"].Value;

            //占い結果をランダムに取り出す
            var random = new Random();
            int fortuneIdx = random.Next(3);
            var fortune = _fortunes[fortuneIdx];

            speechText = $"今日の{sign}の運勢は{fortune.Description}。";
            var repromptText = "他にラッキーカラーが占えます。ラッキーカラーを聞きますか？";

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText + repromptText
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "サンプル星占い",
                Content = speechText
            };
            skillResponse.Response.Reprompt = new Reprompt
            {
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = repromptText
                }
            };
            //セッションオブジェクトを取得
            var attributes = skillResponse.SessionAttributes;

            //nullだったらインスタンスを生成
            if (attributes == null)
            {
                attributes = new Dictionary<string, object>();
            }
            //「sign」をキーにしてユーザーの星座を格納
            attributes["sign"] = sign;
            //レスポンスに格納
            skillResponse.SessionAttributes = attributes;

            return skillResponse;
        }

        private SkillResponse LuckyColorIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };


            SkillResponse ComposeReturnToAskFortuneResponse()
            {
                speechText = "そういえばまだ運勢を占っていませんでしたね。";
                speechText += "今日の運勢を占います。" +
                              "たとえば、ふたご座の運勢を教えてと聞いてください";

                skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = speechText
                };
                skillResponse.Response.Reprompt = new Reprompt
                {
                    OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = speechText
                    }
                };

                return skillResponse;
            }

            //保存された情報がない場合(sessionAttributesがない)
            if (skillRequest.Session.Attributes == null)
            {
                return ComposeReturnToAskFortuneResponse();
            }

            //セッションオブジェクトを取り出す
            Dictionary<string, object> attributes = skillRequest.Session.Attributes;

            //もう一つ、Attributesの中に目的のキーがない
            if (!attributes.ContainsKey("sign"))
            {
                return ComposeReturnToAskFortuneResponse();
            }



            var random = new Random();
            int luckyColorIdx = random.Next(3);
            var luckyColor = _luckyColors[luckyColorIdx];

            speechText = $"今日の{attributes["sign"]}のラッキーカラーは" +
                $"{luckyColor}です。素敵ないちにちを。";

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "サンプル星占い",
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;//セッション終了を指定

            return skillResponse;
        }

        private SkillResponse HelpIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "今日の運勢を占います。" +
                "例えば、ふたご座の運勢を教えてと聞いてください。";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Reprompt = new Reprompt
            {
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = speechText
                }
            };

            return skillResponse;
        }


        private SkillResponse CancelAndStopIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "サンプル星占い",
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;

            return skillResponse;
        }


        private SkillResponse SessionEndedRequestHandler(SkillRequest skillRequest)
        {
            var sessionEndedRequest = skillRequest.Request as SessionEndedRequest;

            return new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };
        }


        private SkillResponse ErrorHandler(SkillRequest skillRequest)
        {
            var speechText = "すみません。聞き取れませんでした。";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Reprompt = new Reprompt
            {
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = speechText
                }
            };

            return skillResponse;
        }

        #endregion
        //テスト
    }
}
