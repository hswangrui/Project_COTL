namespace I2.Loc
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    public static partial class LocalizationManager
    {

        #region Variables: Misc

        public delegate void OnLocalizeCallback();
        public static event OnLocalizeCallback OnLocalizeEvent;

        private static bool mLocalizeIsScheduled;
        private static bool mLocalizeIsScheduledWithForcedValue;

        public static bool HighlightLocalizedTargets = false;

        public static TMP_FontAsset DyslexicFontAsset { get; set; }
        #endregion

        public static string GetTranslation(string Term, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool allowLocalizedParameters=true)
        {
            string Translation = "";
            TryGetTranslation(Term, out Translation, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage, allowLocalizedParameters);

            return Translation;
        }
        public static string GetTermTranslation(string Term, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool allowLocalizedParameters=true)
        {
            return GetTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage, allowLocalizedParameters);
        }


        public static bool TryGetTranslation(string Term, out string Translation, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool allowLocalizedParameters=true)
        {
            Translation = "";
            if (string.IsNullOrEmpty(Term))
                return false;

            InitializeIfNeeded();

            for (int i = 0, imax = Sources.Count; i < imax; ++i)
            {
                if (Sources[i].TryGetTranslation(Term, out Translation, overrideLanguage))
                {
                    if (applyParameters)
                        ApplyLocalizationParams(ref Translation, localParametersRoot, allowLocalizedParameters);

                    if (IsRight2Left && FixForRTL)
                        Translation = ApplyRTLfix(Translation, maxLineLengthForRTL, ignoreRTLnumbers);
                    return true;
                }
            }

            return false;
        }

        public static T GetTranslatedObject<T>( string AssetName, Localize optionalLocComp=null) where T : Object
        {
            if (optionalLocComp != null)
            {
                return optionalLocComp.FindTranslatedObject<T>(AssetName);
            }

            var obj = FindAsset(AssetName) as T;
            if (obj)
                return obj;

            obj = ResourceManager.pInstance.GetAsset<T>(AssetName);
            return obj;
        }
        
        public static T GetTranslatedObjectByTermName<T>( string Term, Localize optionalLocComp=null) where T : Object
        {
            string    translation = GetTranslation(Term, FixForRTL: false);
            return GetTranslatedObject<T>(translation);
        }
        

        public static string GetAppName(string languageCode)
        {
            if (!string.IsNullOrEmpty(languageCode))
            {
                for (int i = 0; i < Sources.Count; ++i)
                {
                    if (string.IsNullOrEmpty(Sources[i].mTerm_AppName))
                        continue;

                    int langIdx = Sources[i].GetLanguageIndexFromCode(languageCode, false);
                    if (langIdx < 0)
                        continue;

                    var termData = Sources[i].GetTermData(Sources[i].mTerm_AppName);
                    if (termData == null)
                        continue;

                    var appName = termData.GetTranslation(langIdx);
                    if (!string.IsNullOrEmpty(appName))
                        return appName;
                }
            }

            return Application.productName;
        }

        public static void LocalizeAll(bool Force = false)
		{
            LoadCurrentLanguage();

            if (!Application.isPlaying)
			{
				DoLocalizeAll(Force);
				return;
			}
			mLocalizeIsScheduledWithForcedValue |= Force;
            if (mLocalizeIsScheduled)
            {
                return;
            }

            CoroutineManager.Start(Coroutine_LocalizeAll());
		}

		static IEnumerator Coroutine_LocalizeAll()
		{
            CheckForDyslexicFonts();
            if (CurrentLanguage == "English") // && SettingsManager.Settings != null && SettingsManager.Settings.Accessibility.DyslexicFont)
            {
                yield return new WaitWhile(() => DyslexicFontAsset == null);
            }


            mLocalizeIsScheduled = true;
            yield return null;
            mLocalizeIsScheduled = false;
            var force = mLocalizeIsScheduledWithForcedValue;
			mLocalizeIsScheduledWithForcedValue = false;
			DoLocalizeAll(force);
		}


        public static void CheckForDyslexicFonts()
        {
            if (CurrentLanguage == "English" )//&& SettingsManager.Settings != null && SettingsManager.Settings.Accessibility.DyslexicFont)
            {
                DyslexicFontAsset = ResourceManager.pInstance.LoadFromAddressables<TMP_FontAsset>("Fonts/Dyslexic/OpenDyslexic-Regular SDF");
            }
            else if (DyslexicFontAsset != null)
            {
                ResourceManager.pInstance.UnloadAddressable("Fonts/Dyslexic/OpenDyslexic-Regular SDF");
                DyslexicFontAsset = null;
            }
        }


        static void DoLocalizeAll(bool Force = false)
		{
			Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll( typeof(Localize) );
			for (int i=0, imax=Locals.Length; i<imax; ++i)
			{
				Localize local = Locals[i];
				//if (ObjectExistInScene (local.gameObject))
				local.OnLocalize(Force);
			}
			if (OnLocalizeEvent != null)
				OnLocalizeEvent ();
			//ResourceManager.pInstance.CleanResourceCache();
            #if UNITY_EDITOR
                RepaintInspectors();
            #endif
        }

        #if UNITY_EDITOR
        static void RepaintInspectors()
        {
            var assemblyEditor      = Assembly.GetAssembly(typeof(Editor));
            var typeInspectorWindow = assemblyEditor.GetType("UnityEditor.InspectorWindow");
            if (typeInspectorWindow != null)
            {
                typeInspectorWindow.GetMethod("RepaintAllInspectors", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            }
        }
#endif

        public static void SetupFonts()
        {
            LoadFont("Fonts/LocalisedFonts/Chinese(Simplified)/Chinese(Simplified)-NotoSansSC-Regular SDF");
            LoadFont("Fonts/LocalisedFonts/Chinese(Tranditional)/Chinese(Traditional)-NotoSansTC-Regular SDF");
            LoadFont("Fonts/LocalisedFonts/Korean/NotoSansKR-Regular SDF");
            LoadFont("Fonts/LocalisedFonts/Japanese/NotoSansJP-Regular SDF");
            LoadFont("Fonts/LocalisedFonts/Japanese/NotoSans-Regular SDF");
           
        }

        private static void LoadFont(string path)
        {
            TMP_FontAsset tMP_FontAsset = null;
           // tMP_FontAsset = ResourceManager.pInstance.LoadFromAddressables<TMP_FontAsset>(path);

            tMP_FontAsset = Resources.Load<TMP_FontAsset>(path);
            if (tMP_FontAsset == null)
            {
                Debug.LogError("Font at path: " + path + " was not loaded correctly! Some fonts will be missing!");
            }
            if (tMP_FontAsset != null && !TMP_Settings.fallbackFontAssets.Contains(tMP_FontAsset))
            {
                TMP_Settings.fallbackFontAssets.Add(tMP_FontAsset);
            }
        }



        public static List<string> GetCategories ()
		{
			List<string> Categories = new List<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				Sources[i].GetCategories(false, Categories);
			return Categories;
		}



		public static List<string> GetTermsList ( string Category = null )
		{
			if (Sources.Count==0)
				UpdateSources();

			if (Sources.Count==1)
				return Sources[0].GetTermsList(Category);

			HashSet<string> Terms = new HashSet<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				Terms.UnionWith( Sources[i].GetTermsList(Category) );
			return new List<string>(Terms);
		}

		public static TermData GetTermData( string term )
		{
            InitializeIfNeeded();

			TermData data;
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				data = Sources[i].GetTermData(term);
				if (data!=null)
					return data;
			}

			return null;
		}
        public static TermData GetTermData(string term, out LanguageSourceData source)
        {
            InitializeIfNeeded();

            TermData data;
            for (int i = 0, imax = Sources.Count; i < imax; ++i)
            {
                data = Sources[i].GetTermData(term);
                if (data != null)
                {
                    source = Sources[i];
                    return data;
                }
            }

            source = null;
            return null;
        }

    }
}
