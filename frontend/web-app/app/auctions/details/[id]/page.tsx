import React from 'react'
import { getDetailedViewData } from '@/app/actions/auctionActions'
import Heading from '@/app/components/Heading';
import CountdownTimer from '../../CountdownTimer';
import CarImage from '../../CarImage';
import DetailedSpecs from './DetailedSpects';
import { getCurrentUser } from '@/app/actions/authActions';
import EditButton from './EditButton';

export default async function Details({params} : {params: {id: string}}) {

  const details = await getDetailedViewData(params.id);
  const user = await getCurrentUser();

  return (
    <div>
      <div className='flex justify-between'>
        <div className='flex items-center gap-3'>
          <Heading title={`${details.make} ${details.model}`} />
          {user?.username === details.seller && (
            <EditButton id={details.id} />
          )}
        </div>
        
        <div className='flex gap-3'>
          <h3 className='text-2xl font-semibold'>Time remaining:</h3>
          <CountdownTimer auctionEnd={details.auctionEnd} />
        </div>
      </div>

      <div className='grid grid-cols-2 gap-6 mt-3'>
          <div className='w-full bg-gray-200 rounded-lg aspect-h-10 aspect-w-16 relative overflow-hidden'>
            <CarImage imageUrl={details.imageUrl} />
          </div>

          <div className='border-2 rounded-lg p-2 bg-gray-100 '>
            <Heading title='Bids' />
          </div>
      
      </div>
      
      <div className="mt-3 grid grid-cols-1 rounded-lg">
          <DetailedSpecs auction={details} />
      </div>

    </div>
  )
}
