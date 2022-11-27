function correctRTImage(filepath, index)
	volume = spm_vol(filepath);
	volumeData = spm_read_vols(volume);
	volumeData = reorient3D(volumeData, index);
	spm_write_vol(volume, volumeData);
	clear volume volumeData;
end