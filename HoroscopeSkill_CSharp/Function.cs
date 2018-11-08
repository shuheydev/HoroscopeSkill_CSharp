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
            new FortuneScore("good","���݂��ŗǂ��ł��傤"),
            new FortuneScore("normal","���ӂ��ŕ��ʂł��傤"),
            new FortuneScore("bad","���ЂƂŃC�}�C�`�ł��傤"),
        };

        private readonly string[] _luckyColors =
        {
            "��",
            "�s���N",
            "�I�����W",
            "�u���[",
            "���F",
            "���F",
            "��",
            "��",
            "�O���[��",
            "�������C�G���[",
            "�z���C�g",
            "�`���R�[���O���["
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
                //�^�X�C�b�`�̗��p
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




        #region �e�C���e���g�A���N�G�X�g�ɑΉ����鏈����S�����郁�\�b�h����

        private SkillResponse HoroscopeIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            //StarSign�X���b�g����l�����o���܂��B
            var sign = intentRequest.Intent.Slots["StarSign"].Value;

            //�肢���ʂ������_���Ɏ��o��
            var random = new Random();
            int fortuneIdx = random.Next(3);
            var fortune = _fortunes[fortuneIdx];

            speechText = $"������{sign}�̉^����{fortune.Description}�B";
            var repromptText = "���Ƀ��b�L�[�J���[���肦�܂��B���b�L�[�J���[�𕷂��܂����H";

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText + repromptText
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "�T���v�����肢",
                Content = speechText
            };
            skillResponse.Response.Reprompt = new Reprompt
            {
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = repromptText
                }
            };
            //�Z�b�V�����I�u�W�F�N�g���擾
            var attributes = skillResponse.SessionAttributes;

            //null��������C���X�^���X�𐶐�
            if (attributes == null)
            {
                attributes = new Dictionary<string, object>();
            }
            //�usign�v���L�[�ɂ��ă��[�U�[�̐������i�[
            attributes["sign"] = sign;
            //���X�|���X�Ɋi�[
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
                speechText = "���������΂܂��^�������Ă��܂���ł����ˁB";
                speechText += "�����̉^����肢�܂��B" +
                              "���Ƃ��΁A�ӂ������̉^���������Ăƕ����Ă�������";

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

            //�ۑ����ꂽ��񂪂Ȃ��ꍇ(sessionAttributes���Ȃ�)
            if (skillRequest.Session.Attributes == null)
            {
                return ComposeReturnToAskFortuneResponse();
            }

            //�Z�b�V�����I�u�W�F�N�g�����o��
            Dictionary<string, object> attributes = skillRequest.Session.Attributes;

            //������AAttributes�̒��ɖړI�̃L�[���Ȃ�
            if (!attributes.ContainsKey("sign"))
            {
                return ComposeReturnToAskFortuneResponse();
            }



            var random = new Random();
            int luckyColorIdx = random.Next(3);
            var luckyColor = _luckyColors[luckyColorIdx];

            speechText = $"������{attributes["sign"]}�̃��b�L�[�J���[��" +
                $"{luckyColor}�ł��B�f�G�Ȃ����ɂ����B";

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "�T���v�����肢",
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;//�Z�b�V�����I�����w��

            return skillResponse;
        }

        private SkillResponse HelpIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "�����̉^����肢�܂��B" +
                "�Ⴆ�΁A�ӂ������̉^���������Ăƕ����Ă��������B";

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
                Title = "�T���v�����肢",
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
            var speechText = "���݂܂���B�������܂���ł����B";

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
        //�e�X�g
    }
}
