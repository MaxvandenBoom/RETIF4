function outMat = correctRTImages(files, index)
	
	% read all volumes
	volumes = spm_vol(files);
	volumes = [volumes{:}]';
	volumeData = spm_read_vols(volumes);
	
	% reorient all volumes
	volumeData = reorient3D_On4DMat(volumeData, index);
	
	% write the files (unfortunately has to be done one by one)
	for i=1:size(files,1)
		singleVolume = squeeze(volumeData(:,:,:,i));
		spm_write_vol(volumes(i), singleVolume);
	end
	
end