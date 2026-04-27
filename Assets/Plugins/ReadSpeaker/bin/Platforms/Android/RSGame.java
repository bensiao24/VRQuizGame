package com.readspeaker.rsgame;

import kr.co.voiceware.java.vtapi.*;
import kr.co.voiceware.vtlicensemodule.*;
import java.util.*;
import java.io.File;


public class RSGame {

    public byte[] result;
    public int engineCount;
    ArrayList<EngineInfo> engineList;
    static VoiceText voicetext;
    static boolean verboseDebug;
    private boolean isUsedLicenseKey = false;

    public void getEngines() {
        this.engineCount = voicetext.vtapiGetEngineInfo().size();
        HashMap<String, EngineInfo> engineInfo = voicetext.vtapiGetEngineInfo();
        ArrayList<EngineInfo> tmp = new ArrayList<EngineInfo>();
        for (Map.Entry<String, EngineInfo> e : engineInfo.entrySet()) {
            tmp.add(e.getValue());
        }
        this.engineList = tmp;
    }

    public String[] getEngineInfoOfIndex(int index) {
        EngineInfo engine = engineList.get(index);
        String[] data = new String[8];
        data[0] = engine.getSpeaker();
        data[1] = engine.getType();
        data[2] = engine.getLang();
        data[3] = engine.getGender();
        data[4] = engine.getDb_path();
        data[5] = engine.getVersion();
        data[6] = Integer.toString(engine.getSampling());
        data[7] = Integer.toString(engine.getChannel());
        return data;
    }

    EngineInfo getEngineInfoByNameAndType(String speaker, int sampleRate, String type) {
        String key = speaker + "-" + sampleRate + "-" + type;
        HashMap<String, EngineInfo> engines = voicetext.vtapiGetEngineInfo();
	    List keys = new ArrayList(engines.keySet());
	    for(int i = 0; i < keys.size(); i++){
	        System.out.println("ReadSpeakerTTS: Engine in hashmap: " + keys.get(i));
	    }
	    return engines.get(key);
    }

    public void loadTTS(String licensePath, String voicePath, VtLicenseSetting licenseModule, boolean verboseDebug) {
        this.verboseDebug = verboseDebug;
        voicetext = new VoiceText();
        // Set license folder to avoid errors being logged.
        voicetext.vtapiSetLicenseFolder(licensePath);
        try {
            voicetext.vtapiSetCallbackForLogFilter(0);
            voicetext.vtapiInit(voicePath);
            voicetext.vtapiLoadEngineInfo();
        } catch (Exception e) {
            System.out.println("ReadSpeakerTTS: Init Error: " + e.getMessage());
        }
    }

    public void exitTTS() {
        voicetext.vtapiExit();
    }

    public static byte[] concat(byte[] first, byte[] second) {
        byte[] result = Arrays.copyOf(first, first.length + second.length);
        System.arraycopy(second, 0, result, first.length, second.length);
        return result;
    }

    public void textToBuffer(String text, String speaker, int sampleRate, String type, int volume, int pitch, int speed, int pause, int commaPause, int textType, String licensePath) {

        long handle = 0;
        boolean ssml = textType == 128;
        int ret = -1;

        result = new byte[0];

	    handle = voicetext.vtapiCreateHandle();
	
	    try{
	        voicetext.vtapiSetAttr(handle, Constants.AttrFlags.ATTR_VOLUME, volume);
	        voicetext.vtapiSetAttr(handle, Constants.AttrFlags.ATTR_PITCH, pitch);
	        voicetext.vtapiSetAttr(handle, Constants.AttrFlags.ATTR_SPEED, speed);
	        voicetext.vtapiSetAttr(handle, Constants.AttrFlags.ATTR_PAUSE, pause);
	        voicetext.vtapiSetAttr(handle, Constants.AttrFlags.ATTR_COMMAPAUSE, commaPause);
        } catch (Exception e){
            System.out.println("ReadSpeakerTTS: SetAttribute error: " + e.getMessage());
        }

        EngineInfo engine = getEngineInfoByNameAndType(speaker, sampleRate, type);

        File licenseFile = new File(licensePath);

        if(licenseFile.exists()){
            voicetext.vtapiSetLicense(engine, licensePath);
            ret = 0;
        }else{
            ret = voicetext.vtapiSetUserKeyword(engine, "RS_Europe");
            	    
	        if(ret != 0){
	            System.out.println("Failed to set keyword: " + ret + ", exiting");
	            return;
	        }
        }
	
	    try{
	        ret = voicetext.vtapiSetEngineHandle(handle, engine);
	    } catch (Exception e){
	        System.out.println("Failed to set engine handle" + e.getMessage());
	    }
	
	    if(ret != 0){
	        System.out.println("Failed to set engine handle: " + ret);
	        return;
	    }

            try {
                voicetext.vtapiTextToBuffer(handle, text, false, 60000, Constants.OutputFormat.FORMAT_16PCM,
                        new VoiceTextListener() {
                            @Override
                            public void onReadBuffer(byte[] output, int outputSize) {
                                if (outputSize > 0) {
                                    result = concat(result, output);
                                    if(verboseDebug) System.out.println("ReadSpeakerTTS: Synthesized " + outputSize + " bytes");
                                } else {
                                    if(verboseDebug) System.out.println("ReadSpeakerTTS: Output buffer empty.");
                                }
                            }
			
                            @Override
                            public void onError(String reason) {
                                //error
                            }
                        });
            } catch (Exception e) {
                System.out.println("ReadSpeakerTTS: TextToBuffer Error: " + e.getMessage());
            }

            try{
                voicetext.vtapiReleaseEngineHandle(engine);
            } catch (Exception e){
                System.out.println("Failed to release engine handle " + e.getMessage());
            }

            voicetext.vtapiReleaseHandle(handle);
        }
}
