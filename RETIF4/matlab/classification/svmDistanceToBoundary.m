% To get the distance from 'newpoint' to the decision boundary in an SVM.
function f = svmDistanceToBoundary( model, newPoint )
	
	% This is more or less a copy of svmdecision (undocumented MATLAB!), but then with scaling of the new point. -LCMB
	
	% Make notation easier:
	supports	= model.SupportVectors;
	K			= model.KernelFunction;
	args		= model.KernelFunctionArgs;
	alpha		= model.Alpha;
	b			= model.Bias;

	
	if isfield(model.ScaleData,'shift')
		shift	= model.ScaleData.shift;
		scale	= model.ScaleData.scaleFactor;
	else
		shift	= zeros( 1, size( supports, 2 ) );
		scale	= ones( 1, size( supports, 2 ) );
	end

	
	f = K( supports, ( newPoint + shift ) .* scale, args{:})' * alpha(:) + b;
	
end