﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.Networking;

namespace I2.Loc
{
    using TranslationDictionary = Dictionary<string, TranslationQuery>;

    public class TranslationJob_POST : TranslationJob_WWW
    {
        TranslationDictionary _requests;
        Action<TranslationDictionary, string> _OnTranslationReady;

        public TranslationJob_POST(TranslationDictionary requests, Action<TranslationDictionary, string> OnTranslationReady)
        {
            _requests = requests;
            _OnTranslationReady = OnTranslationReady;

            var data = GoogleTranslation.ConvertTranslationRequest(requests, false);

            UnityEngine.WWWForm form = new UnityEngine.WWWForm();
            form.AddField("action", "Translate");
            form.AddField("list", data[0]);

            www = UnityWebRequest.Post(LocalizationManager.GetWebServiceURL(), form);
            I2Utils.SendWebRequest(www);
        }

        public override eJobState GetState()
        {
            if (www != null && www.isDone)
            {
                ProcessResult(www.downloadHandler.data, www.error);
                www.Dispose();
                www = null;
            }

            return mJobState;
        }

        public void ProcessResult(byte[] bytes, string errorMsg)
        {
            if (!string.IsNullOrEmpty(errorMsg))
            {
                // check for 
                //if (errorMsg.Contains("rewind"))  // "necessary data rewind wasn't possible"
                mJobState = eJobState.Failed;                    
            }
            else
            {
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                errorMsg = GoogleTranslation.ParseTranslationResult(wwwText, _requests);
                if (_OnTranslationReady!=null)
                    _OnTranslationReady(_requests, errorMsg);
                mJobState = eJobState.Succeeded;
            }
        }
    }
}