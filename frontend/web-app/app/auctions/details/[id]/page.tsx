import React from 'react'
import { getDetailedViewData } from '@/app/actions/auctionActions'
import Heading from '@/app/components/Heading';
import CountdownTimer from '../../CountdownTimer';
import CarImage from '../../CarImage';

export default async function Details({params} : {params: {id: string}}) {

  const details = await getDetailedViewData(params.id);

  return (
    <div>
      <div className='flex justify-between'>
        <Heading title={`${details.make} ${details.model}`} />
        <div className='flex gap-3'>
          <h3 className='text-2xl font-semibold'>Time remaining:</h3>
          <CountdownTimer auctionEnd={details.auctionEnd} />
        </div>
      </div>
      <div className='grid grid-cols-2 gap-6 mt-3'>
          <div className='w-full bg-gray-200 aspect-[4/3] aspect-w-16 rounded-lg overflow-hidden'>
            <CarImage imageUrl={details.imageUrl} />
          </div>
      </div>
      
    </div>
  )
}
