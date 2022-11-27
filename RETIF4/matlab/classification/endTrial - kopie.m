predictedLabel = 0;

if (trStart > 0 && size(rtData, 2) > 0)

	% store the sample index at end of the trial
	trEnd = size(rtData, 2);

	% update the stop index in the list of start and stop indices
	rtDataTrialIndices(size(rtDataTrialIndices,1), 2) = trEnd;

	% extract the data to classify on
	trData = rtData(:,trStart:end);

	% store the data to classify on in the list of trials
	rtDataTrials{length(rtDataTrials) + 1,1} = trData;

	% determine the means (over time)
	trDataMean = mean(trData, 1);
	
	% determine the highest value
	[~, I] = max(trDataMean);
	
	% retrieve the peakData and transpose
	peakData = trData(:, I)';
	
	% store the peak mean
	peakMean = mean(peakData);
	rtDataTrialMaxMeans = vertcat(rtDataTrialMaxMeans, peakMean);
	
	% classify
	[predictedLabel, ~] = multisvmclassify(SVMModel, peakData);
	
end