function outMat = ITKAlignImages(files)
	
	% read all volumes
	volumes = spm_vol(files);
	volumes = [volumes{:}]';
	volumeData = spm_read_vols(volumes);
	
	% make the data single instead of double (the boldreg function requires this)
	volumeData = single(volumeData);

	% take the first image as the realignment reference
	reference = squeeze(volumeData(:,:,:,1));
	
	% loop through volumes (second till last)
	motionList = [0, 0, 0, 0, 0, 0];
	for i=2:size(volumeData, 4)
	
		% realign
		[regdata, estmotion]	= boldreg(reference, squeeze(volumeData(:,:,:,i)), 50, 2, 5000 );
		
		% store the movement estimates and the volume data
		motionList = vertcat(motionList, estmotion');
		volumeData(:,:,:,i) = regdata;
		
	end
	
	% write the files (unfortunately has to be done one by one)
	for i=1:size(files,1)
		singleVolume = squeeze(volumeData(:,:,:,i));
		spm_write_vol(volumes(i), singleVolume);
	end
	
	% save the motion estimations
	%save('c:\output.mat','motionList');
	
end