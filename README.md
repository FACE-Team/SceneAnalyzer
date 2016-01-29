# SceneAnalyzer
Perception System for Social Robots

##### Copyright FACE Team, Research Center E. Piaggio, Univeristy of Pisa 2016 www.faceteam.it

# REQUIREMENTS:

- Visual Studio v2012
- .NET v4.5
- Yarp v2.3.63 (PRE-COMPILED VERSION)
- Kinect XBOX 360 and related drivers
- Kinect SDK v1.8

# HOW TO START

Please cut and copy the FACETools folder that you will find in the /SceneAnalyzer folder to the folder of higher level, 
i.e. from /[your_path]/SceneAnalyzer to /[your_path] for creating /[your_path]/FACETools
Then compile the FACE Tools project

# TROUBLESHOOTING

Please ensure that you are using the English (UK) Format in 'Region and Language' from Control Panel, otherwise signs and points for decimals will disappear from the data provided by the SA.
In our tests we experienced several difficulties using virtual machines. Therefore, if possible, don’t use them. There are problems with Windows libraries of the Kinect and the functioning is not guaranteed.

# ID ASSIGNMENT

The assignment of ID and name related to the detected subjects in the scene is temporary user-guided. In order to avoid crashes or other kind of problems during the execution of the module, we put a timer within which the user has to insert 'name' and 'id'. You will have a few seconds to digit the information since the moment in which the subject is detected, otherwise the pop-up window about that subject will disappear (this is for avoiding image noise setback), in any case the program will continue to run. 
Please always run the program and tick the "Rec. Subject" option BEFORE you start the experiment.

#SHORE

Information about the emotional state of the subjects is estimated through facial expressions analysis performed through the sophisticated high-speed object recognition engine called SHORE, developed by Fraunhofer Institute for Integrated Circuits 

###Note about SHORE license
The software can be licensed based on individual or bulk license contracts. The University of Pisa has a Software Evaluation License agreement with Fraunhofer Institute (http://www.fraunhofer.de/en.html) for the employment of SHORE™ software. 
In order to be used, each user will have to directly ask for the license from the same institute. 


# YARP PORTS

 - /SceneAnalyzer/MetaSceneXML:o (in this port we stream the Metascene as an XML file deserialized in a string)
 - /SceneAnalyzer/MetaSceneOPC:o (in this port we stream the Metascene as an OPC format string)
 - /SceneAnalyzer/Sentence:i
 - /SceneAnalyzer/LookAt:i

[the name of these ports can be modified, and new ports can be instantiated, in the 'app.config' whithin the SceneAnalyzer Project]

# Version
1.4

#PUBLICATIONS

@inproceedings{mazzei2012hefes,
  title={Hefes: An hybrid engine for facial expressions synthesis to control human-like androids and avatars},
  author={Mazzei, Daniele and Lazzeri, Nicole and Hanson, David and De Rossi, Danilo},
  booktitle={Biomedical Robotics and Biomechatronics (BioRob), 2012 4th IEEE RAS \& EMBS International Conference on},
  pages={195--200},
  year={2012},
  organization={IEEE}
}

@article{Lazzeri2013393,
	title = {Towards a believable social robot},
	author = {Lazzeri, N., Mazzei, D., Zaraki, A., De Rossi, D.},
	url = {http://www.scopus.com/inward/record.url?eid=2-s2.0-84880712333&partnerID=40&md5=724a58c54727725eff32f0338306f45e},
	year = {2013},
	date = {2013-01-01},
	journal = {Lecture Notes in Computer Science (including subseries Lecture Notes in Artificial Intelligence and Lecture Notes in Bioinformatics)},
	volume = {8064 LNAI},
	pages = {393-395},
	abstract = {Two perspectives define a human being in his social sphere: appearance and behaviour. The aesthetic aspect is the first significant element that impacts a communication while the behavioural aspect is a crucial factor in evaluating the ongoing interaction. In particular, we have more expectations when interacting with anthropomorphic robots and we tend to define them believable if they respect human social conventions. Therefore researchers are focused both on increasingly anthropomorphizing the embodiment of the robots and on giving the robots a realistic behaviour. This paper describes our research on making a humanoid robot socially interacting with human beings in a believable way. © 2013 Springer-Verlag Berlin Heidelberg.},
note = {cited By 1},
pubstate = {published},
tppubtype = {article}
}

# LICENSE
In order to make this code usable in closed source not-commercial applications under specified conditions, this code is licensed under GPL 3 with linking exception (http://www.gnu.org/licenses/gcc-exception-3.1.en.html).
The exception dosen't permit the use of this code for commercial applications also in form of executable or binary code. 
This exception is also applied if you wish to combine this code with a proprietary code necessary for the running of a product.
Moreover, this exception specifies the details of the claiming necessary for the use of this code.
Any work based, or that links, this code need to claim the FACE Team by using the link www.faceteam.it
The following scientifc publications need also to be cited in all the scientific/accademic publications where this or any derived code is used.


For further information please feel free to contact us at info@faceteam.it
