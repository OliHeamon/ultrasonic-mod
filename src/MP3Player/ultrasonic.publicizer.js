import {createPublicizer} from "publicizer";

export const publicizer = createPublicizer("UltraSonic");

publicizer.createAssembly("tModLoader").publicizeAll();
publicizer.createAssembly("FNA").publicizeAll();