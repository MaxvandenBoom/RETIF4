function regData = ITKAlignImage(referenceData, sourceFilepath, reorient)
	
	% read volume
	volume = spm_vol(sourceFilepath);
	volumeData = spm_read_vols(volume);
	volumeData = reorient3D(volumeData, reorient);
	
	% make the data single instead of double (the boldreg function requires this)
	volumeData = single(volumeData);
	
	% realign
	[regData, estmotion]	= boldreg(referenceData, volumeData, 50, 2, 5000 );
	
end